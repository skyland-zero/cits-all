using Cits;
using Cits.DI;
using Microsoft.Extensions.Caching.Hybrid;
using MyApi.Domain.Models;

namespace MyApi.Domain.Identities;

public class UserManager : ISelfSingletonService
{
    private const int MaxAccessFailedCount = 5;
    private readonly HybridCache _hybridCache;
    private readonly IFreeSql _freeSql;

    private readonly HybridCacheEntryOptions _lockedOutCacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };

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
    public async Task<(bool, string)> IsLockedOutAsync(string username)
    {
        var cache = await _hybridCache.GetOrCreateAsync(
            GetLockoutEndCacheKey(username),
            _ => ValueTask.FromResult(new UserLockedOutCacheModel(username)),
            _lockedOutCacheEntryOptions);

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
    public async Task<int> AccessFailedAsync(string username)
    {
        var cntCacheKey = GetAccessFailedCountCacheKey(username);
        var cnt = await _hybridCache.GetOrCreateAsync(
            cntCacheKey,
            _ => ValueTask.FromResult(0),
            _lockedOutCacheEntryOptions);
        cnt++;

        if (cnt >= MaxAccessFailedCount)
        {
            await _hybridCache.SetAsync(
                GetLockoutEndCacheKey(username),
                new UserLockedOutCacheModel(username, DateTimeOffset.Now.AddMinutes(10)),
                _lockedOutCacheEntryOptions);
            throw new UserFriendlyException("密码错误次数过多，账户已锁定10分钟");
        }

        await _hybridCache.SetAsync(cntCacheKey, cnt, _lockedOutCacheEntryOptions);
        return MaxAccessFailedCount - cnt;
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
}