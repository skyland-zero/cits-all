namespace Cits.Jwt.Models;

public class RefreshTokenCacheModel
{
    public RefreshTokenCacheModel()
    {
    }

    public RefreshTokenCacheModel(Guid userId, Guid sessionId, TimeSpan expire)
    {
        UserId = userId;
        SessionId = sessionId;
        Expire = expire;
    }

    /// <summary>
    ///     用户id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     登录会话id
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    ///     到期时间
    /// </summary>
    public TimeSpan Expire { get; set; }
}
