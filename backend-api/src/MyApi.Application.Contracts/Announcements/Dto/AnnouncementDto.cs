using Cits.Dtos;
using MyApi.Domain.Shared.Announcements;

namespace MyApi.Application.Announcements.Dto;

public class AnnouncementDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string ContentHtml { get; set; } = string.Empty;

    public AnnouncementPriority Priority { get; set; }

    public bool IsPublished { get; set; }

    public bool PopupOnLogin { get; set; }

    public bool VisibleToAll { get; set; }

    public DateTime? PublishTime { get; set; }

    public DateTime? ExpireTime { get; set; }

    public List<Guid> RoleIds { get; set; } = [];

    public List<Guid> OrganizationUnitIds { get; set; } = [];

    public bool IsRead { get; set; }

    public DateTime CreationTime { get; set; }

    public string CreatorUserName { get; set; } = string.Empty;
}

public class QueryAnnouncementDto : PagedRequestDto
{
    public string? Keyword { get; set; }

    public bool? IsPublished { get; set; }

    public AnnouncementPriority? Priority { get; set; }
}

public class AnnouncementCreateUpdateDto
{
    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string ContentHtml { get; set; } = string.Empty;

    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;

    public bool IsPublished { get; set; }

    public bool PopupOnLogin { get; set; }

    public bool VisibleToAll { get; set; } = true;

    public DateTime? PublishTime { get; set; }

    public DateTime? ExpireTime { get; set; }

    public List<Guid> RoleIds { get; set; } = [];

    public List<Guid> OrganizationUnitIds { get; set; } = [];
}

public class AnnouncementPublishDto
{
    public bool IsPublished { get; set; }
}
