using Cits;
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
    public string Logout()
    {
        return _accountAppService.Logout();
    }

    [HttpPost("refresh-token")]
    public async Task<LoginOutput> RefreshTokenAsync(string refreshToken)
    {
        return await _accountAppService.RefreshTokenAsync(refreshToken);
    }
}