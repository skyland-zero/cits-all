using System.ComponentModel.DataAnnotations;
using Cits.Entities;
using FreeSql.DataAnnotations;

namespace MyApi.Domain.Identities;

/// <summary>
/// 在线用户会话。
/// </summary>
[Table(Name = "sys_identity_online_user_sessions")]
[Index("idx_online_session_id", nameof(SessionId), true)]
[Index("idx_online_user_revoked", nameof(UserId) + "," + nameof(IsRevoked), false)]
[Index("idx_online_last_active", nameof(LastActiveTime), false)]
[Index("idx_online_expire", nameof(ExpireTime), false)]
public class IdentityOnlineUserSession : EntityBase
{
    /// <summary>
    /// 会话标识，写入 JWT session_id claim。
    /// </summary>
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    [MaxLength(50)]
    [Column(StringLength = 50)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column(StringLength = 50)]
    public string Surname { get; set; } = string.Empty;

    [MaxLength(80)]
    [Column(StringLength = 80)]
    public string? Ip { get; set; }

    [MaxLength(500)]
    [Column(StringLength = 500)]
    public string? UserAgent { get; set; }

    public DateTime LoginTime { get; set; }

    public DateTime LastActiveTime { get; set; }

    public DateTime ExpireTime { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedTime { get; set; }

    [MaxLength(200)]
    [Column(StringLength = 200)]
    public string? RevokedReason { get; set; }
}
