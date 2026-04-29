using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace Cits.Entities;

/// <summary>
///     包含指定类型主键的实体
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class Entity<TKey> : IEntity
{
    /// <summary>
    ///     主键
    /// </summary>
    [Key]
    [Description("主键Id")]
    [Column(IsIdentity = true, IsPrimary = true)]
    public virtual TKey Id { get; set; } = default!;
}

/// <summary>
///     主键类型为long的实体
/// </summary>
public class Entity : Entity<Guid>
{
}
