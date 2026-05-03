using Cits.Entities;
using FreeSql.DataAnnotations;

namespace MyApi.Domain.Announcements;

[Index("idx_announcement_read_user", nameof(AnnouncementId) + "," + nameof(UserId), true)]
public class SystemAnnouncementReadRecord : EntityBase
{
    public Guid AnnouncementId { get; set; }

    public Guid UserId { get; set; }

    public DateTime ReadTime { get; set; }
}
