using System.ComponentModel.DataAnnotations;

namespace MyApi.Application.Identities.Dto;

public class UserCreateDto
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
    ///     密码hash
    /// </summary>
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    ///     组织架构Id
    /// </summary>
    public Guid OrganizationUnitId { get; set; }
    
    /// <summary>
    /// 角色
    /// </summary>
    public Guid MainRoleId { get; set; }
}
