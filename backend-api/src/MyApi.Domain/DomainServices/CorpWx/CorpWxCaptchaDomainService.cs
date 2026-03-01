using Microsoft.Extensions.Caching.Hybrid;

namespace MyApi.Domain.DomainServices.CorpWx;

public class CorpWxCaptchaDomainService
{
    private readonly HybridCache _cache;
    private readonly ICorpWxClient _corpWxClient;

    public CorpWxCaptchaDomainService(ICorpWxClient corpWxClient, HybridCache cache)
    {
        _corpWxClient = corpWxClient;
        _cache = cache;
    }

    public async Task SendAsync(string uid, string text)
    {
        var res = await _corpWxClient.SendAppMsg(uid, text);
    }

    /// <summary>
    ///     发送验证码后记录验证码状态
    /// </summary>
    /// <param name="phone"></param>
    /// <returns></returns>
    public async Task<string> SetCodeState(string phone)
    {
        var cacheCountKey = $"wecom_login_cnt_{phone}";
        var cacheCodeKey = $"wecom_login_code_{phone}";
        var cacheErrCntKey = $"wecom_login_code_errcnt_{phone}";
        //发送次数缓存
        var sendCntCache = await _cache.GetOrCreateAsync(cacheCountKey,
            _ => ValueTask.FromResult(0),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1)
            });

        sendCntCache++;
        await _cache.SetAsync(cacheCountKey, sendCntCache, new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(1)
        });

        //缓存验证码
        var code = Random.Shared.Next(100001, 1000000);
        await _cache.SetAsync(cacheCodeKey, code.ToString(), new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(20)
        });
        await _cache.RemoveAsync(cacheErrCntKey); //清除验证码错误次数
        return code.ToString();
    }

    /// <summary>
    ///     检查验证码发送次数
    /// </summary>
    /// <param name="phone"></param>
    /// <returns></returns>
    public async Task<int> GetCodeCount(string phone)
    {
        var cacheCountKey = $"wecom_login_cnt_{phone}";
        //发送次数缓存
        var sendCntCache = await _cache.GetOrCreateAsync(cacheCountKey,
            _ => ValueTask.FromResult(0),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1)
            });

        return sendCntCache;
    }

    public async Task<bool> CheckCodeState(string phone, string code)
    {
        var cacheCodeKey = $"wecom_login_code_{phone}";

        var cacheCode = await _cache.GetOrCreateAsync(cacheCodeKey,
            _ => new ValueTask<string>(string.Empty),
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableLocalCacheWrite |
                        HybridCacheEntryFlags.DisableDistributedCacheWrite
            });

        if (cacheCode == string.Empty) return false;

        var cacheErrCntKey = $"wecom_login_code_errcnt_{phone}";

        //判断验证码是否相等
        if (cacheCode == code)
        {
            await _cache.RemoveAsync(cacheCodeKey);
            await _cache.RemoveAsync(cacheErrCntKey);
            return true;
        }

        //判断验证码错误次数
        var errCntCache = await _cache.GetOrCreateAsync(cacheErrCntKey,
            _ => ValueTask.FromResult(1),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(20)
            });


        // 错误次数大于3次，则验证码无效
        if (errCntCache > 3)
        {
            await _cache.RemoveAsync(cacheCodeKey);
            await _cache.RemoveAsync(cacheErrCntKey);
            return false;
        }

        errCntCache++;
        await _cache.SetAsync(cacheErrCntKey, errCntCache, new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(20)
        });
        return false;
    }
}