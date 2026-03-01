using Cits.Entities;

namespace MyApi.Domain.Identities;

/// <summary>
///     页面管理
/// </summary>
public class IdentityPage : EntityBaseWithSoftDelete
{
    /// <summary>
    ///     父级Id
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     层级（从1开始）
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     页面路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     说明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}