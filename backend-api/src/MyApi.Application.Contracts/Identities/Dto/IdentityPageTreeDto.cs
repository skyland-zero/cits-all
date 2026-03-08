using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class IdentityPageTreeDto : EntityDto<Guid>
{
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     名称
    /// </summary>
    public string? Name { get; set; }

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
    public string? Description { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    ///     启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     子级
    /// </summary>
    public List<IdentityPageTreeDto>? Children { get; set; }
}