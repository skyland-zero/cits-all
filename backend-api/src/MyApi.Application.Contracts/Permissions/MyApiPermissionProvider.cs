using Cits.Permissions;

namespace MyApi.Application.Permissions;

public sealed class MyApiPermissionProvider : IPermissionProvider
{
    public void Define(PermissionDefinitionContext context)
    {
        var group = context.AddGroup(MyApiPermissions.Roles.Default, "角色管理");
        group.AddPermission(MyApiPermissions.Roles.Create, "新增");
        group.AddPermission(MyApiPermissions.Roles.Update, "修改");
        group.AddPermission(MyApiPermissions.Roles.Delete, "删除");
        group.AddPermission(MyApiPermissions.Roles.ManagePermissions, "权限配置");
    }
}