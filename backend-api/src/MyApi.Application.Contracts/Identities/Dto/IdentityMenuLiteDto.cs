using Cits.Dtos;
using MyApi.Domain.Shared.Identities;

namespace MyApi.Application.Identities.Dto;

public class IdentityMenuLiteDto : EntityDto<Guid>
{
    /// <summary>
    ///     页面Id
    /// </summary>
    public Guid? PageId { get; set; }

    /// <summary>
    ///     父级Id
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    ///     层级（从1开始）
    /// </summary>
    public int Level { get; set; } = 1;

    /// <summary>
    ///     标题名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     类型
    /// </summary>
    public IdentityMenuType Type { get; set; }

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     路由地址
    /// </summary>
    public string? Path { get; set; }
}
