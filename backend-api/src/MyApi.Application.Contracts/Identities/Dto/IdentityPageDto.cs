using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class IdentityPageDto : EntityDto<Guid>
{
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     路由名称
    /// </summary>
    public string? RouteName { get; set; }

    /// <summary>
    ///     页面路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     说明
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     启用
    /// </summary>
    public bool Enabled { get; set; } = true;
}
