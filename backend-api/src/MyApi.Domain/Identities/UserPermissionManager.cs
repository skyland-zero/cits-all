using Cits.DI;
using Microsoft.Extensions.Caching.Hybrid;
using MyApi.Domain.Models;
using MyApi.Domain.Shared.Identities;

namespace MyApi.Domain.Identities;

public class UserPermissionManager : ISelfSingletonService
{
    private readonly HybridCache _hybridCache;
    private readonly IFreeSql _freeSql;

    public static readonly string UserRoleCacheKey = "user_role_uid:{0}";
    public static readonly string UserCacheKey = "user_uid:{0}";
    public static readonly string UserRoleMenusCacheKey = "user_role_menu_uid:{0}";
    public static readonly string UserAuthPointCacheKey = "user_auth_point_uid:{0}";
    public static readonly string UserRoleIdsCacheKey = "user_roleids_uid:{0}";
    public static readonly string UserOrganizationUnitCacheKey = "user_organization_unit_uid:{0}";
    public static readonly string UserRoleCacheTag = "role_tag_id:{0}";
    public static readonly string UserOrganizationUnitCacheTag = "organization_unit_tag_id:{0}";

    public static readonly string UserCurrentInfoCacheKey = "user_current_info_uid:{0}";
    public static readonly string UserCurrentMenusCacheKey = "user_current_menus_uid:{0}";
    public static readonly string UserPcMenusCacheKey = "user_pc_menus_uid:{0}";

    private readonly HybridCacheEntryOptions _defaultCacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };

    public UserPermissionManager(HybridCache hybridCache, IFreeSql freeSql)
    {
        _hybridCache = hybridCache;
        _freeSql = freeSql;
    }

    public async ValueTask DeleteUserRoleCacheByIdAsync(Guid roleId)
    {
        var tag = string.Format(UserRoleCacheTag, roleId);
        await _hybridCache.RemoveByTagAsync(tag);
    }

    public async ValueTask DeleteUserOrganizationUnitCacheByIdAsync(Guid id)
    {
        var tag = string.Format(UserOrganizationUnitCacheTag, id);
        await _hybridCache.RemoveByTagAsync(tag);
    }

    public async ValueTask DeleteUserOrganizationUnitCacheByIdsAsync(IEnumerable<Guid> ids)
    {
        var tags = ids.Select(x => string.Format(UserOrganizationUnitCacheTag, x)).ToArray();
        await _hybridCache.RemoveByTagAsync(tags);
    }

    /// <summary>
    /// 删除用户权限缓存
    /// </summary>
    public async ValueTask DeleteUserPermissionCacheAsync(Guid userId)
    {
        var keys = new[]
        {
            string.Format(UserRoleIdsCacheKey, userId),
            string.Format(UserRoleCacheKey, userId),
            string.Format(UserRoleMenusCacheKey, userId),
            string.Format(UserAuthPointCacheKey, userId),
            string.Format(UserCacheKey, userId),
            string.Format(UserCurrentInfoCacheKey, userId),
            string.Format(UserCurrentMenusCacheKey, userId),
            string.Format(UserPcMenusCacheKey, userId)
        };
        await _hybridCache.RemoveAsync(keys);
    }

    public async ValueTask<UserCacheModel> GetUserAsync(Guid userId)
    {
        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserCacheKey, userId),
            async cancel => await _freeSql.Select<IdentityUser>().WhereDynamic(userId).FirstAsync<UserCacheModel>(cancel),
            _defaultCacheEntryOptions);
    }

    public async ValueTask<UserRoleCacheModel> GetUserRolesAsync(Guid userId)
    {
        //先获取用户id用于创建tag
        var roleIds = await GetUserRoleIdsAsync(userId);
        var tags = roleIds.Select(x => string.Format(UserRoleCacheTag, x)).ToArray();
        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserRoleCacheKey, userId),
            async cancel => await GetUserRolesFromDatabaseAsync(userId),
            _defaultCacheEntryOptions,
            tags);
    }

    private async ValueTask<UserRoleCacheModel> GetUserRolesFromDatabaseAsync(Guid userId)
    {
        var roles = await _freeSql.Select<IdentityUserRole, IdentityRole>()
            .LeftJoin((a, b) => a.RoleId == b.Id)
            .Where((a, b) => a.UserId == userId)
            .ToListAsync((a, b) => new UserRoleItemCacheModel
            {
                Id = b.Id,
                Name = b.Name,
                Code = b.Code
            });
        return new UserRoleCacheModel
        {
            UserId = userId,
            Roles = roles,
            RoleCodes = roles.Select(x => x.Code).ToHashSet()
        };
    }

    public async ValueTask<List<Guid>> GetUserRoleIdsAsync(Guid userId)
    {
        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserRoleIdsCacheKey, userId),
            async cancel => await GetUserRoleIdsFromDatabaseAsync(userId),
            _defaultCacheEntryOptions);
    }

    private async ValueTask<List<Guid>> GetUserRoleIdsFromDatabaseAsync(Guid userId)
    {
        return await _freeSql.Select<IdentityUserRole>().Where(x => x.UserId == userId).ToListAsync(x => x.RoleId);
    }

    public async Task<List<Guid>> GetUserIdsByRoleIdAsync(Guid roleId)
    {
        return await _freeSql.Select<IdentityUserRole>().Where(x => x.RoleId == roleId).ToListAsync(x => x.UserId);
    }

    public async Task<List<Guid>> GetAllUserIdsAsync()
    {
        return await _freeSql.Select<IdentityUser>().Where(x => x.IsActive && !x.IsDeleted).ToListAsync(x => x.Id);
    }

    public async ValueTask<UserRoleMenuCacheModel> GetUserRoleMenusAsync(Guid userId)
    {
        //先获取用户id用于创建tag
        var roleIds = await GetUserRoleIdsAsync(userId);
        var tags = roleIds.Select(x => string.Format(UserRoleCacheTag, x)).ToArray();
        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserRoleMenusCacheKey, userId),
            async cancel => await GetUserRoleMenusFromDatabaseAsync(userId),
            _defaultCacheEntryOptions,
            tags);
    }

    /// <summary>
    /// 根据用户菜单缓存的角色tag删除关联的所有用户菜单缓存
    /// </summary>
    /// <param name="roleIds"></param>
    /// <returns></returns>
    public ValueTask RemoveUserRoleMenusCacheAsync(IEnumerable<Guid> roleIds)
    {
        var tags = roleIds.Select(x => string.Format(UserRoleCacheTag, x)).ToArray();
        return _hybridCache.RemoveByTagAsync(tags);
    }

    private async ValueTask<UserRoleMenuCacheModel> GetUserRoleMenusFromDatabaseAsync(Guid userId)
    {
        var roleIds = await GetUserRoleIdsAsync(userId);
        var menus = await _freeSql.Select<IdentityMenu, IdentityPage>()
            .LeftJoin((a, b) => a.PageId == b.Id)
            .Where((a, b) => a.Type != IdentityMenuType.AuthPoint)
            .Where((a, b) => a.Roles.AsSelect().Any(x => roleIds.Contains(x.Id)))
            .OrderBy((a, b) => a.Order)
            .ToListAsync((a, b) => new MenuCacheModel
            {
                Id = a.Id,
                PageId = a.PageId,
                ParentId = a.ParentId,
                Level = a.Level,
                HideInBreadcrumb = a.HideInBreadcrumb,
                HideInMenu = a.HideInMenu,
                HideInTab = a.HideInTab,
                Icon = a.Icon,
                IframeSrc = a.IframeSrc,
                KeepAlive = a.KeepAlive,
                Link = a.Link,
                OpenInNewWindow = a.OpenInNewWindow,
                Order = a.Order,
                Query = a.Query,
                Name = a.Name,
                Type = a.Type,
                Enabled = a.Enabled,
                Path = a.Path,
                Redirect = a.Redirect,
                AffixTab = a.AffixTab,
                AffixTabOrder = a.AffixTabOrder,
                Component = b.Path
            });
        var roles = await GetUserRolesFromDatabaseAsync(userId);
        return new UserRoleMenuCacheModel { Roles = roles, Menus = menus };
    }

    public async ValueTask<UserAuthPointCacheModel> GetUserAuthPointAsync(Guid userId)
    {
        //先获取用户id用于创建tag
        var roleIds = await GetUserRoleIdsAsync(userId);
        var tags = roleIds.Select(x => string.Format(UserRoleCacheTag, x)).ToArray();
        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserAuthPointCacheKey, userId),
            async cancel => await GetUserAuthPointFromDatabaseAsync(userId),
            _defaultCacheEntryOptions,
            tags);
    }

    private async ValueTask<UserAuthPointCacheModel> GetUserAuthPointFromDatabaseAsync(Guid userId)
    {
        var roleIds = await GetUserRoleIdsAsync(userId);
        var points = await _freeSql.Select<IdentityMenu>()
            .Where(a => a.Type == IdentityMenuType.AuthPoint && a.Path != null)
            .Where(a => a.Roles.AsSelect().Any(x => roleIds.Contains(x.Id)))
            .ToListAsync(a => a.Path!);
        var roles = await GetUserRolesFromDatabaseAsync(userId);

        return new UserAuthPointCacheModel { Roles = roles, Points = points.ToHashSet() };
    }

    public async ValueTask<UserOrganizationUnitCacheModel> GetUserOrganizationUnitAsync(Guid userId)
    {
        //先获取用户id用于创建tag
        var roleIds = await GetUserRoleIdsAsync(userId);
        var tags = roleIds.Select(x => string.Format(UserRoleCacheTag, x)).ToArray();
        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserOrganizationUnitCacheKey, userId),
            async cancel => await GetUserOrganizationUnitFromDatabaseAsync(userId),
            _defaultCacheEntryOptions,
            tags);
    }

    public ValueTask DeleteUserOrganizationUnitCacheAsync(Guid userId)
    {
        return _hybridCache.RemoveAsync(string.Format(UserOrganizationUnitCacheKey, userId));
    }

    private async ValueTask<UserOrganizationUnitCacheModel> GetUserOrganizationUnitFromDatabaseAsync(Guid userId)
    {
        var deptId = await _freeSql.Select<IdentityUser>().WhereDynamic(userId).FirstAsync(x => x.OrganizationUnitId);
        var org = await _freeSql.Select<IdentityOrganizationUnit>().WhereDynamic(deptId)
            .FirstAsync<UserOrganizationUnitCacheModel>();
        return org;
    }
}