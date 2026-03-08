using Cits;
using Cits.Dtos;
using Mapster;
using MapsterMapper;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

public class IdentityMenuAppService : IIdentityMenuAppService
{
    private readonly IFreeSql _freeSql;
    private readonly UserPermissionManager _userPermissionManager;
    private readonly UserPermissionCachePreWarmer _preWarmer;

    public IdentityMenuAppService(IFreeSql freeSql, UserPermissionManager userPermissionManager, UserPermissionCachePreWarmer preWarmer)
    {
        _freeSql = freeSql;
        _userPermissionManager = userPermissionManager;
        _preWarmer = preWarmer;
    }

    // [Authorize(BasicPermissions.Menus.Default)]
    public async Task<IdentityMenuDto> GetAsync(Guid id)
    {
        var entity = await _freeSql.Select<IdentityMenu>()
            .WhereDynamic(id)
            .FirstAsync<IdentityMenuDto>();
        if (entity == null)
        {
            throw new UserFriendlyException("未找到数据");
        }

        return entity;
    }

    // [Authorize(BasicPermissions.Menus.Default)]
    public async Task<PagedResultDto<IdentityMenuDto>> GetListAsync(GetIdentityMenusInput input)
    {
        var query = _freeSql.Select<IdentityMenu>();

        var count = await query.CountAsync();
        if (count == 0)
        {
            return new PagedResultDto<IdentityMenuDto>(count, []);
        }

        query = query.OrderByDescending(x => x.CreationTime).PageBy(input);
        var list = await query.ToListAsync<IdentityMenuDto>();

        return new PagedResultDto<IdentityMenuDto>(count, list);
    }

    public async Task<PagedResultDto<IdentityMenuLiteDto>> GetLiteListAsync(GetIdentityMenusInput input)
    {
        var query = _freeSql.Select<IdentityMenu>()
            .WhereIf(input.ParentId.HasValue, x => x.ParentId == input.ParentId);

        var count = await query.CountAsync();
        if (count == 0)
        {
            return new PagedResultDto<IdentityMenuLiteDto>(count, []);
        }

        query = query.OrderByDescending(x => x.Id).PageBy(input);
        var list = await query.ToListAsync<IdentityMenuLiteDto>();

        return new PagedResultDto<IdentityMenuLiteDto>(count, list);
    }

    // [Authorize(BasicPermissions.Menus.Create)]
    public async Task CreateAsync(IdentityMenuCreateDto input)
    {
        var entity = input.Adapt<IdentityMenu>();
        if (!string.IsNullOrWhiteSpace(input.RouteName))
        {
            if (await _freeSql.Select<IdentityMenu>().AnyAsync(x => x.RouteName == input.RouteName))
            {
                throw new UserFriendlyException("路由名称已存在");
            }
        }

        if (input.ParentId == null || input.ParentId == Guid.Empty)
        {
            if (await _freeSql.Select<IdentityMenu>().AnyAsync(x => x.Level == 1 && x.Name == input.Name))
            {
                throw new UserFriendlyException("名称已存在");
            }
        }
        else
        {
            var parent = await _freeSql.Select<IdentityMenu>().Where(x => x.Id == input.ParentId).FirstAsync();
            if (parent == null)
            {
                throw new UserFriendlyException("父级节点不存在");
            }

            entity.Level = parent.Level + 1;
        }

        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
    }

    public async Task MultiCreateAsync(List<IdentityMenuCreateDto> inputs)
    {
        foreach (var input in inputs)
        {
            var entity = input.Adapt<IdentityMenu>();
            if (!string.IsNullOrWhiteSpace(input.RouteName))
            {
                if (await _freeSql.Select<IdentityMenu>().AnyAsync(x => x.RouteName == input.RouteName))
                {
                    throw new UserFriendlyException($"路由名称 {input.RouteName} 已存在");
                }
            }

            if (input.ParentId == null || input.ParentId == Guid.Empty)
            {
                if (await _freeSql.Select<IdentityMenu>().AnyAsync(x => x.Level == 1 && x.Name == input.Name))
                {
                    throw new UserFriendlyException($"名称 {input.Name} 已存在");
                }
            }
            else
            {
                var parent = await _freeSql.Select<IdentityMenu>().Where(x => x.Id == input.ParentId).FirstAsync();
                if (parent == null)
                {
                    throw new UserFriendlyException("父级节点不存在");
                }

                entity.Level = parent.Level + 1;
            }

            await _freeSql.Insert(entity).ExecuteAffrowsAsync();
        }
    }

    // [Authorize(BasicPermissions.Menus.Update)]
    public async Task UpdateAsync(Guid id, IdentityMenuUpdateDto input)
    {
        if (!string.IsNullOrWhiteSpace(input.RouteName))
        {
            if (await _freeSql.Select<IdentityMenu>().AnyAsync(x => x.RouteName == input.RouteName && x.Id != id))
            {
                throw new UserFriendlyException("路由名称已存在");
            }
        }

        var entity = await _freeSql.Select<IdentityMenu>().Where(x => x.Id == id).FirstAsync();

        var hasChanges = DtoEntityComparer.HasChanges(input, entity);
        input.Adapt(entity);
        await _freeSql.Update<IdentityMenu>().SetSource(entity).ExecuteAffrowsAsync();
        if (hasChanges)
        {
            await RemoveRoleMenuCacheAsync(id);
        }
    }

    /// <summary>
    /// 删除菜单关联到的角色再关联到的用户的菜单缓存
    /// </summary>
    /// <param name="id"></param>
    private async ValueTask RemoveRoleMenuCacheAsync(Guid id)
    {
        var roleIds = await _freeSql.Select<IdentityRoleMenu>()
            .Where(x => x.MenuId == id)
            .ToListAsync(x => x.RoleId);
        await _userPermissionManager.RemoveUserRoleMenusCacheAsync(roleIds);
        foreach (var roleId in roleIds)
        {
            _preWarmer.PreWarmByRoleId(roleId);
        }
    }

    // [Authorize(BasicPermissions.Menus.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        if (await _freeSql.Select<IdentityMenu>().AnyAsync(x => x.ParentId == id))
        {
            throw new UserFriendlyException("包含子级，无法删除");
        }

        using var uow = _freeSql.CreateUnitOfWork();
        await uow.Orm.Update<IdentityMenu>()
            .Set(x => x.IsDeleted == true)
            .Where(x => x.Id == id)
            .ExecuteAffrowsAsync();
        await uow.Orm.Delete<IdentityRoleMenu>()
            .Where(x => x.MenuId == id)
            .ExecuteAffrowsAsync();
        uow.Commit();
        await RemoveRoleMenuCacheAsync(id);
    }

    public async Task<ListResultDto<IdentityMenuTreeDto>> GetTreeAsync()
    {
        var all = await _freeSql.Select<IdentityMenu>()
            .OrderBy(x => x.Order)
            .ToListAsync<IdentityMenuTreeDto>();
        return new ListResultDto<IdentityMenuTreeDto>(BuildMenuTree(all));
    }

    private static List<IdentityMenuTreeDto> BuildMenuTree(List<IdentityMenuTreeDto> flatList)
    {
        var dict = flatList.ToDictionary(x => x.Id);
        var tree = new List<IdentityMenuTreeDto>();

        foreach (var item in flatList)
        {
            if (item.ParentId == null || !dict.ContainsKey(item.ParentId.Value))
            {
                tree.Add(item); // 根节点
            }
            else
            {
                var parent = dict[item.ParentId.Value];
                parent.Children ??= new List<IdentityMenuTreeDto>();
                parent.Children.Add(item);
            }
        }

        return tree;
    }

    public async Task<ListResultDto<TreeSelectGuidDto>> GetTreeSelectAsync()
    {
        var list = await _freeSql.Select<IdentityMenu>().ToListAsync(x => new TreeSelectGuidDto
        {
            ParentId = x.ParentId,
            Value = x.Id,
            Label = x.Name,
            Sort = x.Order,
            ExtStr1 = x.Path,
            ExtInt1 = (int)x.Type
        });
        var rootNodes = list.Where(n => n.ParentId == null).ToList();
        foreach (var root in rootNodes)
        {
            // 递归构建子树
            root.Children = GetChildren(root, list);
        }

        // 按Sort排序根节点
        return new ListResultDto<TreeSelectGuidDto>(rootNodes.OrderBy(n => n.Sort).ToList());
    }

    private static List<TreeSelectGuidDto> GetChildren(TreeSelectGuidDto parent, List<TreeSelectGuidDto> allNodes)
    {
        // 查找直接子节点
        var children = allNodes
            .Where(n => n.ParentId == parent.Value)
            .OrderBy(n => n.Sort)
            .ToList();

        // 递归处理子节点
        foreach (var child in children)
        {
            child.Children = GetChildren(child, allNodes);
        }

        return children;
    }
}