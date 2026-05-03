using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.OnlineUsers;
using MyApi.Application.OnlineUsers.Dto;

namespace MyApi.HttpApi.Controllers.OnlineUsers;

/// <summary>
/// 在线用户管理。
/// </summary>
[Route("api/monitor/online-users")]
[Authorize]
public class OnlineUserController : BaseApiController
{
    private readonly IOnlineUserAppService _onlineUserAppService;

    public OnlineUserController(IOnlineUserAppService onlineUserAppService)
    {
        _onlineUserAppService = onlineUserAppService;
    }

    [HttpGet]
    public Task<PagedResultDto<OnlineUserSessionDto>> GetListAsync([FromQuery] QueryOnlineUserSessionDto input)
    {
        return _onlineUserAppService.GetListAsync(input);
    }

    [HttpPost("{sessionId:guid}/revoke")]
    public Task RevokeAsync(Guid sessionId, [FromBody] RevokeOnlineUserSessionDto input)
    {
        return _onlineUserAppService.RevokeAsync(sessionId, input);
    }

    [HttpPost("users/{userId:guid}/revoke")]
    public Task RevokeUserAsync(Guid userId, [FromBody] RevokeOnlineUserSessionDto input)
    {
        return _onlineUserAppService.RevokeUserAsync(userId, input);
    }
}
