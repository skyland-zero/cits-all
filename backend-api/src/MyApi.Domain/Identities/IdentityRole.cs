using Cits.Entities;
using FreeSql.DataAnnotations;

namespace MyApi.Domain.Identities;

public class IdentityRole : EntityBaseWithSoftDelete<Guid>
{
    public int EntityVersion { get; set; }

    /// <summary>
    ///     角色名称
    /// </summary>
    public string Name { get; protected internal set; } = string.Empty;

    /// <summary>
    ///     角色唯一编码
    /// </summary>
    public string Code { get; protected internal set; } = string.Empty;

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
    
    /// <summary>
    /// Menus导航属性
    /// </summary>
    [Navigate(ManyToMany = typeof(IdentityRoleMenu))]
    public virtual ICollection<IdentityMenu> Menus { get; set; } = new List<IdentityMenu>();
}
