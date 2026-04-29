using System.ComponentModel.DataAnnotations;

namespace MyApi.Application.Identities.Dto;

public class OrganizationUnitCreateUpdateDto
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
    public string Code { get; set; } = string.Empty;

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    ///     父级ID
    /// </summary>
    public Guid? ParentId { get; set; }
}
