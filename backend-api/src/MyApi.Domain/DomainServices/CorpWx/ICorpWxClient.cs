namespace MyApi.Domain.DomainServices.CorpWx;

public interface ICorpWxClient
{
    /// <summary>
    ///     获取企业微信api token
    /// </summary>
    /// <returns></returns>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    ///     根据Code获取企业微信用户id
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<string?> GetUserIdAsync(string code);

    /// <summary>
    ///     发送应用消息
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<bool> SendAppMsg(string uid, string text);
}