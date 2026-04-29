using FreeSql.DataAnnotations;

namespace Cits.Entities;

public class EntityBase<TKey> : Entity<TKey>
{
    /// <summary>
    ///     创建时间
    /// </summary>
    [Column(ServerTime = DateTimeKind.Local, CanUpdate = false)]
    public DateTime CreationTime { get; set; }

    /// <summary>
    ///     创建人
    /// </summary>
    public TKey CreatorUserId { get; set; } = default!;

    /// <summary>
    ///     创建人名称
    /// </summary>
    public string CreatorUserName { get; set; } = string.Empty;

    /// <summary>
    ///     修改时间
    /// </summary>
    [Column(ServerTime = DateTimeKind.Local)]
    public DateTime LastModificationTime { get; set; }

    /// <summary>
    ///     修改人
    /// </summary>
    public TKey LastModifierUserId { get; set; } = default!;

    /// <summary>
    ///     最后修改人名称
    /// </summary>
    public string LastModifierUserName { get; set; } = string.Empty;
}

public class EntityBase : EntityBase<Guid>
{
}
