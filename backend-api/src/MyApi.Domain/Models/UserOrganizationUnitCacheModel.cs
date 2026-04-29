using System.ComponentModel.DataAnnotations;

namespace MyApi.Domain.Models;

public class UserOrganizationUnitCacheModel
{
    /// <summary>
    ///     名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     编码
    /// </summary>
    [MaxLength(20)]
    public string? Code { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }


    /// <summary>
    ///     父级ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     路径
    /// </summary>
    [MaxLength(1000)]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     名称路径
    /// </summary>
    [MaxLength(2000)]
    public string NamePath { get; set; } = string.Empty;

    /// <summary>
    ///     层级
    /// </summary>
    public int Level { get; set; }
}
