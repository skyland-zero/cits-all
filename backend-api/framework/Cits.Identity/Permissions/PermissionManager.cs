using Microsoft.Extensions.DependencyInjection;

namespace Cits.Permissions;

public class PermissionManager
{
    private readonly IFreeSql _freeSql;

    public PermissionManager(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public async Task SyncPermissionsAsync(IServiceProvider serviceProvider)
    {
        var currentPermissions = GetAllFromProviders(serviceProvider);
        await SyncPermissionsAsync(currentPermissions);
    }

    private List<PermissionGroupModel> GetAllFromProviders(IServiceProvider serviceProvider)
    {
        var context = new PermissionDefinitionContext();
        var providers = serviceProvider.GetServices<IPermissionProvider>();

        foreach (var provider in providers.OrderBy(p => p.GetType().Name))
        {
            provider.Define(context);
        }

        return context.GetGroups().ToList();
    }

    public async Task SyncPermissionsAsync(IEnumerable<PermissionGroupModel> permissionGroups)
    {
        var groups = permissionGroups.ToList();

        var dbGroups = await _freeSql.Select<PermissionGroup>().ToListAsync();
        var dbPermissions = await _freeSql.Select<Permission>().ToListAsync();

        var addGroups = new List<PermissionGroup>();
        var updateGroups = new List<PermissionGroup>();

        var permissions = new List<PermissionModel>();

        var dbGroupDict = dbGroups.ToDictionary(g => g.Code);
        foreach (var group in groups)
        {
            if (dbGroupDict.TryGetValue(group.Code, out var dbGroup))
            {
                if (!GroupEquals(dbGroup, group))
                {
                    dbGroup.DisplayName = group.DisplayName;
                    dbGroup.IsEnabled = true;
                    updateGroups.Add(dbGroup);
                }

                // 移除已匹配的项，剩余项即为需要删除的
                dbGroupDict.Remove(group.Code);
            }
            else
            {
                // 新增的项
                addGroups.Add(new PermissionGroup
                {
                    Code = group.Code,
                    DisplayName = group.DisplayName,
                    IsEnabled = true
                });
            }

            permissions.AddRange(group.GetPermissions());
        }

        // 剩余未匹配的数据库项即为需要删除的
        var deleteGroups = dbGroupDict.Values.ToList();
        deleteGroups.ForEach(g => g.IsEnabled = false);

        var addPermissions = new List<Permission>();
        var updatePermissions = new List<Permission>();

        var dbPermissionDict = dbPermissions.ToDictionary(g => g.GroupCode + g.Code);
        foreach (var permission in permissions)
            if (dbPermissionDict.TryGetValue(permission.GroupCode + permission.Code, out var dbPermission))
            {
                if (!PermissionEquals(dbPermission, permission))
                {
                    dbPermission.DisplayName = dbPermission.DisplayName;
                    dbPermission.IsEnabled = true;
                    updatePermissions.Add(dbPermission);
                }

                // 移除已匹配的项，剩余项即为需要删除的
                dbPermissionDict.Remove(permission.GroupCode + permission.Code);
            }
            else
            {
                // 新增的项
                addPermissions.Add(new Permission
                {
                    Code = permission.Code,
                    DisplayName = permission.DisplayName,
                    GroupCode = permission.GroupCode,
                    IsEnabled = true
                });
            }

        var deletePermissions = dbPermissionDict.Values.ToList();
        deletePermissions.ForEach(g => g.IsEnabled = false);

        using var uow = _freeSql.CreateUnitOfWork();
        //保存权限组数据
        await uow.Orm.Insert(addGroups).NoneParameter().ExecuteAffrowsAsync();
        await uow.Orm.Update<PermissionGroup>().SetSource(updateGroups).NoneParameter().ExecuteAffrowsAsync();
        await uow.Orm.Update<PermissionGroup>().SetSource(deleteGroups).NoneParameter().ExecuteAffrowsAsync();
        //保存权限数据
        await uow.Orm.Insert(addPermissions).NoneParameter().ExecuteAffrowsAsync();
        await uow.Orm.Update<Permission>().SetSource(updatePermissions).NoneParameter().ExecuteAffrowsAsync();
        await uow.Orm.Update<Permission>().SetSource(deletePermissions).NoneParameter().ExecuteAffrowsAsync();

        uow.Commit();
    }

    private bool GroupEquals(PermissionGroup group1, PermissionGroupModel group2)
    {
        return group1.Code == group2.Code &&
               group1.DisplayName == group2.DisplayName && 
               group1.IsEnabled;
    }

    private bool PermissionEquals(Permission model1, PermissionModel model2)
    {
        return model1.Code == model2.Code &&
               model1.DisplayName == model2.DisplayName && 
               model1.IsEnabled;
    }
}