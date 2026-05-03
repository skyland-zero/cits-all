using Cits;
using Cits.DI;
using Microsoft.Extensions.Caching.Hybrid;
using MyApi.Domain.Models;

namespace MyApi.Domain.Identities;

public class UserManager : ISelfSingletonService
{
    private readonly HybridCache _hybridCache;
    private readonly IFreeSql _freeSql;

    public UserManager(HybridCache hybridCache, IFreeSql freeSql)
    {
        _hybridCache = hybridCache;
        _freeSql = freeSql;
    }

    public async Task<IdentityUser?> FindByUserName(string username)
    {
        return await _freeSql.Select<IdentityUser>().Where(x => x.UserName == username).FirstAsync();
    }

    public async Task<IdentityUser?> FindByUserId(Guid userId)
    {
        return await _freeSql.Select<IdentityUser>().WhereDynamic(userId).FirstAsync();
    }

    /// <summary>
    /// 账户是否已锁定
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<(bool, string)> IsLockedOutAsync(string username, int lockoutMinutes)
    {
        var lockoutOptions = BuildLockoutCacheOptions(lockoutMinutes);
        var cache = await _hybridCache.GetOrCreateAsync(
            GetLockoutEndCacheKey(username),
            _ => ValueTask.FromResult(new UserLockedOutCacheModel(username)),
            lockoutOptions);

        if (cache.LockoutEnd.HasValue && cache.LockoutEnd.Value > DateTimeOffset.Now)
        {
            return (true, $"账户已锁定，解锁时间：{cache.LockoutEnd.Value.LocalDateTime:yyyy-MM-dd HH:mm}");
        }

        return (false, "");
    }

    /// <summary>
    ///     记录账号密码验证错误，并且返回剩余锁定次数
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    /// <exception cref="UserFriendlyException"></exception>
    public async Task<int> AccessFailedAsync(string username, int maxAccessFailedCount, int lockoutMinutes)
    {
        maxAccessFailedCount = Math.Max(1, maxAccessFailedCount);
        lockoutMinutes = Math.Max(1, lockoutMinutes);
        var lockoutOptions = BuildLockoutCacheOptions(lockoutMinutes);
        var cntCacheKey = GetAccessFailedCountCacheKey(username);
        var cnt = await _hybridCache.GetOrCreateAsync(
            cntCacheKey,
            _ => ValueTask.FromResult(0),
            lockoutOptions);
        cnt++;

        if (cnt >= maxAccessFailedCount)
        {
            await _hybridCache.SetAsync(
                GetLockoutEndCacheKey(username),
                new UserLockedOutCacheModel(username, DateTimeOffset.Now.AddMinutes(lockoutMinutes)),
                lockoutOptions);
            throw new UserFriendlyException($"密码错误次数过多，账户已锁定{lockoutMinutes}分钟");
        }

        await _hybridCache.SetAsync(cntCacheKey, cnt, lockoutOptions);
        return maxAccessFailedCount - cnt;
    }

    public async Task ResetAccessFailedCountAsync(string username)
    {
        await _hybridCache.RemoveAsync(GetAccessFailedCountCacheKey(username));
        await _hybridCache.RemoveAsync(GetLockoutEndCacheKey(username));
    }

    private string GetAccessFailedCountCacheKey(string username)
    {
        return $"access_failed_count_{username}";
    }

    private string GetLockoutEndCacheKey(string username)
    {
        return $"user_locked_out_{username}";
    }

    private static HybridCacheEntryOptions BuildLockoutCacheOptions(int lockoutMinutes)
    {
        var expiration = TimeSpan.FromMinutes(Math.Max(1, lockoutMinutes));
        return new HybridCacheEntryOptions
        {
            Expiration = expiration,
            LocalCacheExpiration = expiration
        };
    }
}
