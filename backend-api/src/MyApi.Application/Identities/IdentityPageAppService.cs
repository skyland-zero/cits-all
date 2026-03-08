using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

[Authorize]
public class IdentityPageAppService : IIdentityPageAppService
{
    private readonly IFreeSql _freeSql;

    public IdentityPageAppService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    // [Authorize(BasicPermissions.Pages.Default)]
    public async Task<IdentityPageDto> GetAsync(Guid id)
    {
        var data = await _freeSql.Select<IdentityPage>().WhereDynamic(id).FirstAsync<IdentityPageDto>();
        return data;
    }

    // [Authorize(BasicPermissions.Pages.Default)]
    public async Task<PagedResultDto<IdentityPageDto>> GetListAsync(GetIdentityPagesInput input)
    {
        var query = _freeSql.Select<IdentityPage>()
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(x.Name))
            .WhereIf(!input.Path.IsNullOrWhiteSpace(), x => x.Path.Contains(x.Path))
            .WhereIf(!input.Description.IsNullOrWhiteSpace(), x => x.Description.Contains(x.Description));

        var count = await query.CountAsync();
        if (count == 0)
        {
            return new PagedResultDto<IdentityPageDto>(count, []);
        }

        query = query.OrderByDescending(x => x.CreationTime).PageBy(input);
        var list = await query.ToListAsync<IdentityPageDto>();

        return new PagedResultDto<IdentityPageDto>(count, list);
    }

    // [Authorize(BasicPermissions.Pages.Create)]
    public async Task CreateAsync(IdentityPageCreateUpdateDto input)
    {
        var entity = input.Adapt<IdentityPage>();
        if (!input.RouteName.IsNullOrWhiteSpace())
        {
            if (await _freeSql.Select<IdentityPage>().AnyAsync(x => x.RouteName == input.RouteName))
            {
                throw new UserFriendlyException("路由名称已存在");
            }
        }

        if (input.ParentId == null || input.ParentId == Guid.Empty)
        {
            if (await _freeSql.Select<IdentityPage>().AnyAsync(x => x.Level == 1 && x.Name == input.Name))
            {
                throw new UserFriendlyException("名称已存在");
            }
        }
        else
        {
            var parent = await _freeSql.Select<IdentityPage>().Where(x => x.Id == input.ParentId).FirstAsync();
            if (parent == null)
            {
                throw new UserFriendlyException("父级节点不存在");
            }

            entity.Level = parent.Level + 1;
        }

        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
    }

    // [Authorize(BasicPermissions.Pages.Update)]
    public async Task UpdateAsync(Guid id, IdentityPageCreateUpdateDto input)
    {
        if (!input.RouteName.IsNullOrWhiteSpace())
        {
            if (await _freeSql.Select<IdentityPage>().AnyAsync(x => x.RouteName == input.RouteName && x.Id != id))
            {
                throw new UserFriendlyException("路由名称已存在");
            }
        }

        var entity = await _freeSql.Select<IdentityPage>().Where(x => x.Id == id).FirstAsync();
        input.Adapt(entity);
        await _freeSql.Update<IdentityPage>().SetSource(entity).ExecuteAffrowsAsync();
    }

    // [Authorize(BasicPermissions.Pages.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        if (await _freeSql.Select<IdentityPage>().AnyAsync(x => x.ParentId == id))
        {
            throw new UserFriendlyException("删除失败：页面包含子级，无法删除");
        }

        if (await _freeSql.Select<IdentityMenu>().AnyAsync(x => x.PageId == id))
        {
            throw new UserFriendlyException("删除失败：菜单中引用了该页面，请先删除关联菜单");
        }

        await _freeSql.Update<IdentityPage>()
            .Set(x => x.IsDeleted == true)
            .WhereDynamic(id)
            .ExecuteAffrowsAsync();
    }

    public async Task<ListResultDto<IdentityPageTreeDto>> GetTreeAsync()
    {
        var list = await _freeSql.Select<IdentityPage>().ToListAsync<IdentityPageTreeDto>();
        return new ListResultDto<IdentityPageTreeDto>(BuildTree(list));
    }

    /// <summary>
    /// 将平面列表转换为树形结构
    /// </summary>
    /// <param name="nodes">所有节点（包含父子关系）</param>
    /// <returns>根节点列表（ParentId为null或Guid.Empty）</returns>
    private List<IdentityPageTreeDto> BuildTree(List<IdentityPageTreeDto>? nodes)
    {
        // 空检查
        if (nodes == null || !nodes.Any())
        {
            return new List<IdentityPageTreeDto>();
        }

        // 用字典加速查找（Key=节点Id，Value=节点对象）
        var nodeDict = nodes.ToDictionary(n => n.Id);

        // 存放根节点（没有父节点的节点）
        var rootNodes = new List<IdentityPageTreeDto>();

        foreach (var node in nodes)
        {
            // 处理根节点
            if (node.ParentId == null || node.ParentId == Guid.Empty)
            {
                rootNodes.Add(node);
                continue;
            }

            // 将当前节点挂载到父节点的Children下
            if (nodeDict.TryGetValue(node.ParentId.Value, out var parentNode))
            {
                parentNode.Children ??= new List<IdentityPageTreeDto>();
                parentNode.Children.Add(node);
            }
            // 可选：处理无效的ParentId（如记录日志或抛出异常）
        }

        // 按Sort字段排序（可选）
        foreach (var node in nodes)
        {
            if (node.Children != null)
            {
                node.Children = node.Children.OrderBy(c => c.Sort).ToList();
            }
        }

        return rootNodes.OrderBy(n => n.Sort).ToList();
    }

    public async Task<ListResultDto<TreeSelectGuidDto>> GetTreeSelectAsync()
    {
        var list = await _freeSql.Select<IdentityPage>()
            .ToListAsync(x => new TreeSelectGuidDto
            {
                ParentId = x.ParentId,
                Value = x.Id,
                Label = x.Name,
                Sort = x.Sort,
                ExtStr1 = x.Path
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