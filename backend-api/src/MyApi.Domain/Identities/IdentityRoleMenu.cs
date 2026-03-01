using Cits.Entities;
using FreeSql.DataAnnotations;

namespace MyApi.Domain.Identities;

/// <summary>
///     角色菜单权限
/// </summary>
public class IdentityRoleMenu : IEntity
{
    /// <summary>
    ///     角色Id
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    ///     菜单Id
    /// </summary>
    public Guid MenuId { get; set; }
    
    [Navigate(nameof(MenuId))]
    public virtual IdentityMenu? Menu { get; set; }

    [Navigate(nameof(RoleId))]
    public virtual IdentityRole? Roles { get; set; }
}