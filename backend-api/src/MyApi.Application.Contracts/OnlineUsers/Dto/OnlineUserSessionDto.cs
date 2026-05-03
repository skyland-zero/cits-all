using Cits.Dtos;

namespace MyApi.Application.OnlineUsers.Dto;

public class OnlineUserSessionDto : EntityDto<Guid>
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string? Ip { get; set; }

    public string? UserAgent { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime LastActiveTime { get; set; }

    public DateTime ExpireTime { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedTime { get; set; }

    public string? RevokedReason { get; set; }

    public bool IsOnline { get; set; }
}

public class QueryOnlineUserSessionDto : PagedRequestDto
{
    public string? Keyword { get; set; }

    public string? Ip { get; set; }

    public bool OnlineOnly { get; set; } = true;
}

public class RevokeOnlineUserSessionDto
{
    public string? Reason { get; set; }
}
