using System.ComponentModel.DataAnnotations;
using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class UserDto : EntityDto<Guid>
{
    /// <summary>
    ///     账号
    /// </summary>
    [MaxLength(20)]
    public string UserName { get; set; }

    /// <summary>
    ///     姓名
    /// </summary>
    [MaxLength(20)]
    public string Surname { get; set; }

    /// <summary>
    ///     是否已激活
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     组织架构Id
    /// </summary>
    public Guid OrganizationUnitId { get; set; }
    
    /// <summary>
    /// 角色
    /// </summary>
    public Guid MainRoleId { get; set; }
}