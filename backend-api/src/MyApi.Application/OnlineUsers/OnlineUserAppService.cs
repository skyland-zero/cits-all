using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Mapster;
using MyApi.Application.OnlineUsers.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.OnlineUsers;

public class OnlineUserAppService : IOnlineUserAppService
{
    private static readonly TimeSpan OnlineWindow = TimeSpan.FromMinutes(5);
    private readonly IFreeSql _freeSql;
    private readonly OnlineUserSessionManager _onlineUserSessionManager;

    public OnlineUserAppService(IFreeSql freeSql, OnlineUserSessionManager onlineUserSessionManager)
    {
        _freeSql = freeSql;
        _onlineUserSessionManager = onlineUserSessionManager;
    }

    public async Task<PagedResultDto<OnlineUserSessionDto>> GetListAsync(QueryOnlineUserSessionDto input)
    {
        var keyword = input.Keyword ?? string.Empty;
        var ip = input.Ip ?? string.Empty;
        var now = DateTime.UtcNow;
        var onlineAfter = now.Subtract(OnlineWindow);

        var query = _freeSql.Select<IdentityOnlineUserSession>()
            .WhereIf(!keyword.IsNullOrWhiteSpace(), x =>
                x.UserName.Contains(keyword) || x.Surname.Contains(keyword))
            .WhereIf(!ip.IsNullOrWhiteSpace(), x => x.Ip != null && x.Ip.Contains(ip))
            .WhereIf(input.OnlineOnly, x => !x.IsRevoked && x.ExpireTime > now && x.LastActiveTime >= onlineAfter);

        var count = await query.CountAsync();
        if (count == 0) return new PagedResultDto<OnlineUserSessionDto>(count, []);

        var list = await query
            .OrderByDescending(x => x.LastActiveTime)
            .PageBy(input)
            .ToListAsync();

        var items = list.ConvertAll(item =>
        {
            var dto = item.Adapt<OnlineUserSessionDto>();
            dto.IsOnline = !item.IsRevoked && item.ExpireTime > now && item.LastActiveTime >= onlineAfter;
            return dto;
        });

        return new PagedResultDto<OnlineUserSessionDto>(count, items);
    }

    public Task RevokeAsync(Guid sessionId, RevokeOnlineUserSessionDto input)
    {
        return _onlineUserSessionManager.RevokeAsync(sessionId, input.Reason ?? "管理员强制下线");
    }

    public Task RevokeUserAsync(Guid userId, RevokeOnlineUserSessionDto input)
    {
        return _onlineUserSessionManager.RevokeUserSessionsAsync(userId, input.Reason ?? "管理员强制下线用户");
    }
}
