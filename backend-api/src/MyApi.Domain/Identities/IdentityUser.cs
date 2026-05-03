using System.ComponentModel.DataAnnotations;
using Cits.Entities;

namespace MyApi.Domain.Identities;

public class IdentityUser : EntityBaseWithSoftDelete
{

    /// <summary>
    ///     密码hash
    /// </summary>
    [MaxLength(500)]
    public required string PasswordHash { get; set; }

    /// <summary>
    ///     账号
    /// </summary>
    [MaxLength(20)]
    public required string UserName { get; set; }

    /// <summary>
    ///     姓名
    /// </summary>
    [MaxLength(20)]
    public required string Surname { get; set; }

    /// <summary>
    ///     组织架构Id
    /// </summary>
    public Guid OrganizationUnitId { get; set; }

    /// <summary>
    ///     是否已激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     是否必须先修改密码
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    ///     密码最后修改时间
    /// </summary>
    public DateTime? PasswordChangedTime { get; set; }

    /// <summary>
    ///     安全戳，密码变更时刷新
    /// </summary>
    [MaxLength(64)]
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
     
    /// <summary>
    /// 角色
    /// </summary>
    public Guid MainRoleId { get; set; }

    /// <summary>
    /// 企业微信用户 ID
    /// </summary>
    [MaxLength(100)]
    public string? CorpWxUserId { get; set; }
}
