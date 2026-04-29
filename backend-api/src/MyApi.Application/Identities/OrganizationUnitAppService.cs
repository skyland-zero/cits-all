using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Cits.IdGenerator;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

[Authorize]
public class OrganizationUnitAppService : IOrganizationUnitService
{
    private readonly IFreeSql _freeSql;
    private readonly IIdGenerator _idGenerator;
    private readonly UserPermissionManager _userPermissionManager;

    public OrganizationUnitAppService(IFreeSql freeSql, IIdGenerator idGenerator,
        UserPermissionManager userPermissionManager)
    {
        _freeSql = freeSql;
        _idGenerator = idGenerator;
        _userPermissionManager = userPermissionManager;
    }

    public async Task<OrganizationUnitDto> GetAsync(Guid id)
    {
        var data = await _freeSql.Select<IdentityOrganizationUnit>().WhereDynamic(id).FirstAsync<OrganizationUnitDto>();
        return data;
    }

    public async Task<PagedResultDto<OrganizationUnitDto>> GetListAsync(GetOrganizationUnitInput input)
    {
        var name = input.Name ?? string.Empty;
        var code = input.Code ?? string.Empty;

        var query = _freeSql.Select<IdentityOrganizationUnit>()
            .WhereIf(!name.IsNullOrWhiteSpace(), x => x.Name.Contains(name))
            .WhereIf(!code.IsNullOrWhiteSpace(), x => x.Code == code);

        var count = await query.CountAsync();
        if (count == 0)
        {
            return new PagedResultDto<OrganizationUnitDto>(count, []);
        }

        query = query.OrderByDescending(x => x.CreationTime).PageBy(input);
        var list = await query.ToListAsync<OrganizationUnitDto>();
        return new PagedResultDto<OrganizationUnitDto>(count, list);
    }

    public async Task CreateAsync(OrganizationUnitCreateUpdateDto input)
    {
        if (await _freeSql.Select<IdentityOrganizationUnit>()
                .AnyAsync(x => x.ParentId == input.ParentId && x.Name == input.Name))
        {
            throw new UserFriendlyException("同层级下名称已存在");
        }

        if (!input.Code.IsNullOrWhiteSpace() &&
            await _freeSql.Select<IdentityOrganizationUnit>().AnyAsync(x => x.Code == input.Code))
        {
            throw new UserFriendlyException("部门编码已存在");
        }

        var entity = new IdentityOrganizationUnit
        {
            Id = _idGenerator.Create()
        };
        input.Adapt(entity);
        await ProcessTreeInfoAsync(entity, input.ParentId);
        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
    }

    public async Task UpdateAsync(Guid id, OrganizationUnitCreateUpdateDto input)
    {
        if (await _freeSql.Select<IdentityOrganizationUnit>().AnyAsync(x =>
                x.ParentId == input.ParentId && x.Id != id && x.Name == input.Name))
        {
            throw new UserFriendlyException("同层级下名称已存在");
        }

        if (!input.Code.IsNullOrWhiteSpace() &&
            await _freeSql.Select<IdentityOrganizationUnit>().AnyAsync(x => x.Id != id && x.Code == input.Code))
        {
            throw new UserFriendlyException("部门编码已存在");
        }

        var entity = await _freeSql.Select<IdentityOrganizationUnit>().WhereDynamic(id).FirstAsync();
        if (entity == null)
        {
            throw new UserFriendlyException("数据存在");
        }

        var hasChanged = DtoEntityComparer.HasChanges(input, entity);
        //当父级Id变更或者名称变更后，需要对应处理子集的层级信息
        var needProcessChildren = (entity.ParentId != input.ParentId) || (entity.Name != input.Name);
        var oldPath = entity.Path;
        input.Adapt(entity);
        await ProcessTreeInfoAsync(entity, input.ParentId);
        if (needProcessChildren)
        {
            await ProcessChildrenAsync(entity, oldPath);
        }

        await _freeSql.Update<IdentityOrganizationUnit>().SetSource(entity).ExecuteAffrowsAsync();
        if (hasChanged) await _userPermissionManager.DeleteUserOrganizationUnitCacheByIdAsync(id); //根据tag删除缓存
    }

    /// <summary>
    /// 处理层级信息
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="parentId"></param>
    /// <exception cref="UserFriendlyException"></exception>
    private async Task ProcessTreeInfoAsync(IdentityOrganizationUnit entity, Guid? parentId)
    {
        if (parentId != null && parentId != Guid.Empty)
        {
            var parent = await _freeSql.Select<IdentityOrganizationUnit>().WhereDynamic(parentId.Value).FirstAsync();
            if (parent == null)
            {
                throw new UserFriendlyException("父级部门不存在");
            }

            entity.Path = $"{parent.Path}{entity.Id},";
            entity.Level = parent.Level + 1;
            entity.NamePath = $"{parent.NamePath}>{entity.Name}";
        }
        else
        {
            entity.Path = $"{entity.Id},";
            entity.Level = 1;
            entity.NamePath = entity.Name;
        }
    }

    /// <summary>
    /// 处理子集层级信息
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="oldPath"></param>
    private async Task ProcessChildrenAsync(IdentityOrganizationUnit entity, string oldPath)
    {
        var children = await _freeSql.Select<IdentityOrganizationUnit>()
            .Where(x => x.Id != entity.Id && x.Path.StartsWith(oldPath))
            .ToListAsync();
        foreach (var item in children)
        {
            item.Path = $"{entity.Path}{item.Id},";
            item.Level = entity.Level + 1;
            item.NamePath = $"{entity.NamePath}>{item.Name}";
        }

        await _freeSql.Update<IdentityOrganizationUnit>().SetSource(children).ExecuteAffrowsAsync();
        await _userPermissionManager.DeleteUserOrganizationUnitCacheByIdsAsync(children.Select(x => x.Id));
    }

    public async Task DeleteAsync(Guid id)
    {
        if (await _freeSql.Select<IdentityUser>().AnyAsync(x => x.OrganizationUnitId == id))
        {
            throw new UserFriendlyException("组织架构已关联有用户，无法删除");
        }


        await _freeSql.Update<IdentityOrganizationUnit>()
            .Set(x => x.IsDeleted == true)
            .WhereDynamic(id)
            .ExecuteAffrowsAsync();
    }

    public async Task<ListResultDto<OrganizationUnitTreeDto>> GetTreeAsync()
    {
        var data = await _freeSql.Select<IdentityOrganizationUnit>().ToListAsync();
        var list = data.Adapt<List<OrganizationUnitTreeDto>>();
        var rootNodes = list.Where(n => n.ParentId == null).ToList();
        foreach (var root in rootNodes)
        {
            // 递归构建子树
            root.Children = GetChildren(root, list);
        }

        // 按Sort排序根节点
        return new ListResultDto<OrganizationUnitTreeDto>(rootNodes.OrderBy(n => n.Sort).ToList());
    }

    private static List<OrganizationUnitTreeDto> GetChildren(OrganizationUnitTreeDto parent,
        List<OrganizationUnitTreeDto> allNodes)
    {
        // 查找直接子节点
        var children = allNodes
            .Where(n => n.ParentId == parent.Id)
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
