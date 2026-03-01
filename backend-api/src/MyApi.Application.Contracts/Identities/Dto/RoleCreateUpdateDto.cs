using System.ComponentModel.DataAnnotations;

namespace MyApi.Application.Identities.Dto;

public class RoleCreateUpdateDto
{
    /// <summary>
    ///     角色名称
    /// </summary>
    [MaxLength(20)]
    public string Name { get; set; }

    /// <summary>
    ///     角色唯一编码
    /// </summary>
    [MaxLength(50)]
    public string Code { get; set; }

    /// <summary>
    ///     A default role is automatically assigned to a new user
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    ///     A static role can not be deleted/renamed
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    ///     菜单Ids
    /// </summary>
    public List<Guid>? MenuIds { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    public string? Description { get; set; }
}