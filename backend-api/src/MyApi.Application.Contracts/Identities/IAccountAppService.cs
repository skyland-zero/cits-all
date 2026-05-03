using Cits.DI;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

/// <summary>
///     账号登录服务
/// </summary>
public interface IAccountAppService : IApplicationService
{
    Task<LoginOutput> LoginAsync(LoginInput input);

    /// <summary>
    ///     退出登录
    /// </summary>
    /// <returns></returns>
    Task<string> LogoutAsync();

    Task<LoginOutput> RefreshTokenAsync(string refreshToken);

    Task ChangePasswordAsync(ChangePasswordDto input);
}
