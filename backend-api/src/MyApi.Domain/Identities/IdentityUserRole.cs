using Cits.Entities;

namespace MyApi.Domain.Identities;

public class IdentityUserRole : Entity
{
    /// <summary>
    ///     用户id
    /// </summary>
    public virtual Guid UserId { get; set; }

    /// <summary>
    ///     角色id
    /// </summary>
    public virtual Guid RoleId { get; set; }
}