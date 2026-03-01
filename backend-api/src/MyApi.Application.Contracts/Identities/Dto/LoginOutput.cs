namespace MyApi.Application.Identities.Dto;

public class LoginOutput
{
    /// <summary>
    ///     Token
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    ///     刷新令牌
    /// </summary>
    public string? RefreshToken { get; set; }
}