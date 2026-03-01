using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class RoleDto : EntityDto<Guid>
{
    /// <summary>
    ///     角色名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     角色唯一编码
    /// </summary>
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
    ///     描述
    /// </summary>
    public string? Description { get; set; }
}