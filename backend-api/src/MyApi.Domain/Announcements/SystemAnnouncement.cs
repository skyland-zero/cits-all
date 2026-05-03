using System.ComponentModel.DataAnnotations;
using Cits.Entities;
using MyApi.Domain.Shared.Announcements;

namespace MyApi.Domain.Announcements;

public class SystemAnnouncement : EntityBaseWithSoftDelete
{
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    public string ContentHtml { get; set; } = string.Empty;

    public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;

    public bool IsPublished { get; set; }

    public bool PopupOnLogin { get; set; }

    public bool VisibleToAll { get; set; } = true;

    public DateTime? PublishTime { get; set; }

    public DateTime? ExpireTime { get; set; }

    /// <summary>
    ///     可见角色 ID JSON 数组。
    /// </summary>
    public string RoleIdsJson { get; set; } = "[]";

    /// <summary>
    ///     可见部门 ID JSON 数组。
    /// </summary>
    public string OrganizationUnitIdsJson { get; set; } = "[]";
}
