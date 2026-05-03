using Cits.DI;
using Cits.Dtos;
using MyApi.Application.OnlineUsers.Dto;

namespace MyApi.Application.OnlineUsers;

public interface IOnlineUserAppService : IApplicationService
{
    Task<PagedResultDto<OnlineUserSessionDto>> GetListAsync(QueryOnlineUserSessionDto input);

    Task RevokeAsync(Guid sessionId, RevokeOnlineUserSessionDto input);

    Task RevokeUserAsync(Guid userId, RevokeOnlineUserSessionDto input);
}
