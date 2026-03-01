using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyApi.Domain.DomainServices.CorpWx.Dto;
using RestSharp;

namespace MyApi.Domain.DomainServices.CorpWx;

/// <summary>
///     企业微信接口客户端
/// </summary>
public class CorpWxClient : ICorpWxClient, IDisposable
{
    private readonly HybridCache _cache;
    private readonly RestClient _client;
    private readonly ILogger<CorpWxClient> _logger;
    private readonly IOptions<CorpWxOptions> _options;

    public CorpWxClient(IOptions<CorpWxOptions> options, ILogger<CorpWxClient> logger, HybridCache cache)
    {
        _options = options;
        _logger = logger;
        _cache = cache;
        var opt = new RestClientOptions("https://qyapi.weixin.qq.com");
        _client = new RestClient(opt);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await _cache.GetOrCreateAsync(GetCacheKey(), async cancel => await GetAccessTokenFromApiAsync(cancel));
    }

    public async Task<string?> GetUserIdAsync(string code)
    {
        var token = await GetAccessTokenAsync();
        if (token is null) _logger.LogError("未获取到企业微信Token");

        var url = $"cgi-bin/auth/getuserinfo?access_token={token}&code={code}";
        var response = await _client.GetAsync<UserInfoResponse>(url);

        if (response is null)
        {
            _logger.LogError("请求失败");
            return null;
        }

        if (response.ErrorCode != 0)
        {
            _logger.LogError($"Error Code: {response.ErrorCode},Error Message: {response.ErrorMessage}");
            return null;
        }

        return response.UserId;
    }

    public async Task<bool> SendAppMsg(string uid, string text)
    {
        var token = await GetAccessTokenAsync();
        if (token == null) return false;

        var url = $"cgi-bin/message/send?access_token={token}";
        var data = new SendAppMsgRequest
        {
            ToUser = uid,
            AgentId = _options.Value.AgentId,
            Text = new SendAppMsgRequest.TextContent { Content = text }
        };

        var response = await _client.PostJsonAsync<SendAppMsgRequest, SendAppMsgResponse>(url, data);

        if (response is null)
        {
            _logger.LogError("请求失败");
            return false;
        }

        if (response.ErrorCode == 42001) //Token过期
        {
            await GetAccessTokenFromApiAsync();
            return await SendAppMsg(uid, text);
        }

        if (response.ErrorCode != 0)
        {
            _logger.LogError($"企业微信发送消息失败：Error Code: {response.ErrorCode},Error Message: {response.ErrorMessage}");
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<string?> GetAccessTokenFromApiAsync(CancellationToken cancellationToken = default)
    {
        var url = $"cgi-bin/gettoken?corpid={_options.Value.CorpId}&corpsecret={_options.Value.CorpSecret}";
        var response = await _client.GetAsync<TokenResponse>(url, cancellationToken);
        if (response is null)
        {
            _logger.LogError("请求失败");
            return null;
        }

        if (response.ErrorCode != 0)
        {
            _logger.LogError($"Error Code: {response.ErrorCode},Error Message: {response.ErrorMessage}");
            return null;
        }

        await _cache.SetAsync(GetCacheKey(), response.AccessToken
            , new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(response.ExpiresIn - 60 ?? 60 * 60)
            }, cancellationToken: cancellationToken);
        return response.AccessToken;
    }

    private string GetCacheKey()
    {
        return $"corp_access_token:{_options.Value.CorpId}";
    }
}