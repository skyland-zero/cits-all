using System.ComponentModel.DataAnnotations;
using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class UserDto : EntityDto<Guid>
{
    /// <summary>
    ///     账号
    /// </summary>
    [MaxLength(20)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    ///     姓名
    /// </summary>
    [MaxLength(20)]
    public string Surname { get; set; } = string.Empty;

    /// <summary>
    ///     是否已激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     是否必须修改密码
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    ///     密码最后修改时间
    /// </summary>
    public DateTime? PasswordChangedTime { get; set; }

    /// <summary>
    ///     组织架构Id
    /// </summary>
    public Guid OrganizationUnitId { get; set; }
    
    /// <summary>
    /// 角色
    /// </summary>
    public Guid MainRoleId { get; set; }
}
