using Cits.Dtos;
using MyApi.Domain.Shared.Identities;

namespace MyApi.Application.Identities.Dto;

public class IdentityMenuDto : EntityDto<Guid>
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
    ///     当前路由在面包屑中不展现
    /// </summary>
    public bool HideInBreadcrumb { get; set; } = false;

    /// <summary>
    ///     当前路由在菜单中不展现
    /// </summary>
    public bool HideInMenu { get; set; } = false;

    /// <summary>
    ///     当前路由在标签页不展现
    /// </summary>
    public bool HideInTab { get; set; } = false;

    /// <summary>
    ///     图标（菜单/tab）
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    ///     iframe 地址
    /// </summary>
    public string? IframeSrc { get; set; }

    /// <summary>
    ///     开启KeepAlive缓存
    /// </summary>
    public bool KeepAlive { get; set; }

    /// <summary>
    ///     外链-跳转路径
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    ///     在新窗口打开
    /// </summary>
    public bool OpenInNewWindow { get; set; }

    /// <summary>
    ///     序号
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     路由参数
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    ///     标题名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     路由名称
    /// </summary>
    public string? RouteName { get; set; }

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

    /// <summary>
    ///     重定向
    /// </summary>
    public string? Redirect { get; set; }

    /// <summary>
    ///     固定标签页
    /// </summary>
    public bool AffixTab { get; set; }

    /// <summary>
    ///     固定标签页序号
    /// </summary>
    public int? AffixTabOrder { get; set; }
}