using Cits.Dtos;
using MyApi.Domain.Shared.Identities;

namespace MyApi.Application.Identities.Dto;

public class IdentityMenuTreeDto : EntityDto<Guid>
{
    /// <summary>
    ///     父级Id
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     序号
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     标题名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     类型
    /// </summary>
    public IdentityMenuType Type { get; set; }

    public string TypeStr => Type.ToDescriptionOrString();

    /// <summary>
    ///     路由地址
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    ///     重定向
    /// </summary>
    public string? Redirect { get; set; }

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     子级
    /// </summary>
    public List<IdentityMenuTreeDto> Children { get; set; } = [];
}
