using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
public class LoginLogController : IdentityBaseApiController
{
    private readonly ILoginLogAppService _loginLogAppService;

    public LoginLogController(ILoginLogAppService loginLogAppService)
    {
        _loginLogAppService = loginLogAppService;
    }

    [HttpGet]
    public Task<PagedResultDto<LoginLogDto>> GetListAsync([FromQuery] GetLoginLogsInput input)
    {
        return _loginLogAppService.GetListAsync(input);
    }
}