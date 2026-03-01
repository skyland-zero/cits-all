namespace Cits.Jwt.Models;

public class RefreshTokenCacheModel
{
    public RefreshTokenCacheModel()
    {
    }

    public RefreshTokenCacheModel(Guid userId, TimeSpan expire)
    {
        UserId = userId;
        Expire = expire;
    }

    /// <summary>
    ///     用户id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    ///     到期时间
    /// </summary>
    public TimeSpan Expire { get; set; }
}