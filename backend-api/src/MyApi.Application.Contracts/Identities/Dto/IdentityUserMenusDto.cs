using Cits.Dtos;

namespace MyApi.Application.Identities.Dto;

public class IdentityUserMenusDto : EntityDto<Guid>
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
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    ///     iframe 地址
    /// </summary>
    public string IframeSrc { get; set; } = string.Empty;

    /// <summary>
    ///     开启KeepAlive缓存
    /// </summary>
    public bool KeepAlive { get; set; }

    /// <summary>
    ///     外链-跳转路径
    /// </summary>
    public string Link { get; set; } = string.Empty;

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
    public string Query { get; set; } = string.Empty;

    /// <summary>
    ///     标题名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     页面路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     子级
    /// </summary>
    public List<IdentityUserMenusDto>? Items { get; set; }
}
