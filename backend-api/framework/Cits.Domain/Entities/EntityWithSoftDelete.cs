namespace Cits.Entities;

/// <summary>
///     带删除标记的实体基类
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class EntityWithSoftDelete<TKey> : Entity<TKey>, IDelete
{
    /// <summary>
    ///     删除标记
    /// </summary>
    public bool IsDeleted { get; set; }
}

/// <summary>
///     默认实现
/// </summary>
public class EntityWithSoftDelete : EntityWithSoftDelete<Guid>
{
}