using System.ComponentModel;

namespace MyApi.Domain.Shared.Identities;

public enum IdentityMenuType
{
    /// <summary>
    ///     分组
    /// </summary>
    [Description("分组")] Group = 1,

    /// <summary>
    ///     菜单
    /// </summary>
    [Description("菜单")] Menu = 2,

    /// <summary>
    ///     权限点
    /// </summary>
    [Description("权限点")] AuthPoint = 3
}