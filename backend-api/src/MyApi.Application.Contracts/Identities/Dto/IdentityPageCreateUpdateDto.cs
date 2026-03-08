using System.ComponentModel.DataAnnotations;

namespace MyApi.Application.Identities.Dto;

public class IdentityPageCreateUpdateDto
{
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    ///     路由名称
    /// </summary>
    public string? RouteName { get; set; }

    /// <summary>
    ///     页面路径
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     说明
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    [Required]
    public int Sort { get; set; }

    /// <summary>
    ///     启用
    /// </summary>
    [Required]
    public bool Enabled { get; set; } = true;
}