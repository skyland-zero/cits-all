using Cits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

public class AccountController : IdentityBaseApiController
{
    private readonly IAccountAppService _accountAppService;

    public AccountController(IAccountAppService accountAppService)
    {
        _accountAppService = accountAppService;
    }

    [HttpPost("login")]
    public async Task<LoginOutput> LoginAsync(LoginInput input)
    {
        return await _accountAppService.LoginAsync(input);
    }

    [HttpPost("logout")]
    public Task<string> LogoutAsync()
    {
        return _accountAppService.LogoutAsync();
    }

    [HttpPost("refresh-token")]
    public async Task<LoginOutput> RefreshTokenAsync(string refreshToken)
    {
        return await _accountAppService.RefreshTokenAsync(refreshToken);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task ChangePasswordAsync(ChangePasswordDto input)
    {
        await _accountAppService.ChangePasswordAsync(input);
    }
}
