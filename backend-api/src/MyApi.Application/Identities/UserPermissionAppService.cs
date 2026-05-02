using Cits;
using Cits.Dtos;
using Mapster;
using Microsoft.Extensions.Caching.Hybrid;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

public class UserPermissionAppService : IUserPermissionAppService
{
    private readonly UserPermissionManager _userPermissionManager;
    private readonly ICurrentUser _currentUser;
    private readonly HybridCache _hybridCache;

    private readonly HybridCacheEntryOptions _defaultCacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };

    public UserPermissionAppService(UserPermissionManager userPermissionManager, ICurrentUser currentUser,
        HybridCache hybridCache)
    {
        _userPermissionManager = userPermissionManager;
        _currentUser = currentUser;
        _hybridCache = hybridCache;
    }

    /// <summary>
    /// 获取登录用户的角色权限Code
    /// </summary>
    /// <returns></returns>
    public async Task<string[]> GetPermissionCodesAsync()
    {
        var points = await _userPermissionManager.GetUserAuthPointAsync(_currentUser.Id);
        return points.Points.ToArray();
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    /// <returns></returns>
    public async Task<CurrentIdentityUserDto?> GetCurrentAsync()
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
        {
            return null;
        }

        return await GetCurrentInfoInternalAsync(userId);
    }

    private async Task<CurrentIdentityUserDto?> GetCurrentInfoInternalAsync(Guid userId)
    {
        var roleIds = await _userPermissionManager.GetUserRoleIdsAsync(userId);
        var tags = roleIds.Select(x => string.Format(UserPermissionManager.UserRoleCacheTag, x)).ToArray();

        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserPermissionManager.UserCurrentInfoCacheKey, userId),
            async cancel =>
            {
                var user = await _userPermissionManager.GetUserAsync(userId);
                var roles = await _userPermissionManager.GetUserRolesAsync(userId);
                var res = user.Adapt<CurrentIdentityUserDto>();
                res.UserId = user.Id;
                res.Roles = roles.RoleCodes.ToArray();
                return res;
            },
            _defaultCacheEntryOptions,
            tags);
    }

    /// <summary>
    /// 获取当前用户菜单权限
    /// </summary>
    /// <returns></returns>
    public async Task<List<IdentityUserMenusDto>> GetCurrentMenusAsync()
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
        {
            return [];
        }

        return await GetCurrentMenusInternalAsync(userId);
    }

    private async Task<List<IdentityUserMenusDto>> GetCurrentMenusInternalAsync(Guid userId)
    {
        var roleIds = await _userPermissionManager.GetUserRoleIdsAsync(userId);
        var tags = roleIds.Select(x => string.Format(UserPermissionManager.UserRoleCacheTag, x)).ToArray();

        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserPermissionManager.UserCurrentMenusCacheKey, userId),
            async cancel =>
            {
                var menu = await _userPermissionManager.GetUserRoleMenusAsync(userId);
                var list = menu.Menus.Adapt<List<IdentityUserMenusDto>>();


                return list.FindAll(x => x.Level == 1)
                    .ConvertAll(item =>
                    {
                        item.Items = GetMenuChildren(item.Id, in list);
                        return item;
                    });
            },
            _defaultCacheEntryOptions,
            tags);
    }

    /// <summary>
    /// 获取当前用户菜单权限-递归获取子级
    /// </summary>
    /// <param name="parentId"></param>
    /// <param name="all"></param>
    /// <returns></returns>
    private List<IdentityUserMenusDto>? GetMenuChildren(Guid parentId, in List<IdentityUserMenusDto> all)
    {
        var items = all.Where(x => x.ParentId == parentId).ToList();
        if (!items.Any())
        {
            return null;
        }

        foreach (var item in items)
        {
            item.Items = GetMenuChildren(item.Id, in all);
        }

        return items;
    }

    public async Task<List<UserPcMenuDto>> GetPcMenusAsync()
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
        {
            return [];
        }

        return await GetPcMenusInternalAsync(userId);
    }

    private async Task<List<UserPcMenuDto>> GetPcMenusInternalAsync(Guid userId)
    {
        var roleIds = await _userPermissionManager.GetUserRoleIdsAsync(userId);
        var tags = roleIds.Select(x => string.Format(UserPermissionManager.UserRoleCacheTag, x)).ToArray();

        return await _hybridCache.GetOrCreateAsync(
            string.Format(UserPermissionManager.UserPcMenusCacheKey, userId),
            async cancel =>
            {
                var menu = await _userPermissionManager.GetUserRoleMenusAsync(userId);
                var list = menu.Menus.Select(x =>
                {
                    var name = !string.IsNullOrWhiteSpace(x.RouteName)
                        ? x.RouteName
                        : (!string.IsNullOrWhiteSpace(x.MenuRouteName)
                            ? x.MenuRouteName
                            : x.Name);

                    return new UserPcMenuDto
                    {
                        Id = x.Id,
                        Level = x.Level,
                        ParentId = x.ParentId,
                        Name = name,
                        Path = x.Path,
                        Redirect = x.Redirect,
                        Component = x.Component,
                        Meta = new UserPcMenuMetaDto
                        {
                            Order = x.Order,
                            Title = x.Name,
                            AffixTab = x.AffixTab,
                            Icon = x.Icon,
                            HideInMenu = x.HideInMenu
                        }
                    };
                }).ToList();

                return list.FindAll(x => x.Level == 1)
                    .ConvertAll(item =>
                    {
                        item.Component = "BasicLayout";
                        item.Children = GetPcMenuChildren(item.Id, in list);
                        return item;
                    });
            },
            _defaultCacheEntryOptions,
            tags);
    }

    public async Task PreWarmCacheAsync(Guid userId)
    {
        await GetCurrentInfoInternalAsync(userId);
        await GetCurrentMenusInternalAsync(userId);
        await GetPcMenusInternalAsync(userId);
    }

    private List<UserPcMenuDto>? GetPcMenuChildren(Guid parentId, in List<UserPcMenuDto> all)
    {
        var items = all.Where(x => x.ParentId == parentId).ToList();
        if (!items.Any())
        {
            return null;
        }

        foreach (var item in items)
        {
            item.Children = GetPcMenuChildren(item.Id, in all);
        }

        return items;
    }
}
