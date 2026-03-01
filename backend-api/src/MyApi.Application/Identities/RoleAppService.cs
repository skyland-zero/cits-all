using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;
using MyApi.Domain.Shared.Identities;

namespace MyApi.Application.Identities;

[Authorize]
public class RoleAppService : IRoleAppService
{
    private readonly IFreeSql _freeSql;
    private readonly UserPermissionManager _userPermissionManager;
    private readonly UserPermissionCachePreWarmer _preWarmer;

    public RoleAppService(IFreeSql freeSql, UserPermissionManager userPermissionManager, UserPermissionCachePreWarmer preWarmer)
    {
        _freeSql = freeSql;
        _userPermissionManager = userPermissionManager;
        _preWarmer = preWarmer;
    }

    public async Task<RoleDto> GetAsync(Guid id)
    {
        var data = await _freeSql.Select<IdentityRole>().WhereDynamic(id).FirstAsync<RoleDto>();
        return data;
    }

    public async Task<PagedResultDto<RoleDto>> GetListAsync(GetRolesInput input)
    {
        var query = _freeSql.Select<IdentityRole>()
            .WhereIf(!input.Name.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Name))
            .WhereIf(!input.Code.IsNullOrWhiteSpace(), x => x.Code == input.Code)
            .WhereIf(input.IsDefault.HasValue, x => x.IsDefault == input.IsDefault)
            .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic);

        var count = await query.CountAsync();
        if (count == 0)
        {
            return new PagedResultDto<RoleDto>(count, []);
        }

        query = query.OrderByDescending(x => x.CreationTime).PageBy(input);
        var list = await query.ToListAsync<RoleDto>();
        return new PagedResultDto<RoleDto>(count, list);
    }

    public async Task CreateAsync(RoleCreateUpdateDto input)
    {
        if (await _freeSql.Select<IdentityRole>().AnyAsync(x => x.Name == input.Name))
        {
            throw new UserFriendlyException("名称已存在");
        }

        if (await _freeSql.Select<IdentityRole>().AnyAsync(x => x.Code == input.Code))
        {
            throw new UserFriendlyException("角色编码已存在");
        }

        var entity = input.Adapt<IdentityRole>();

        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
    }

    public async Task UpdateAsync(Guid id, RoleCreateUpdateDto input)
    {
        if (await _freeSql.Select<IdentityRole>().AnyAsync(x => x.Id != id && x.Name == input.Name))
        {
            throw new UserFriendlyException("名称已存在");
        }

        if (await _freeSql.Select<IdentityRole>().AnyAsync(x => x.Id != id && x.Code == input.Code))
        {
            throw new UserFriendlyException("角色编码已存在");
        }

        var entity = await _freeSql.Select<IdentityRole>().WhereDynamic(id).FirstAsync();
        if (entity == null)
        {
            throw new UserFriendlyException("数据不存在");
        }

        input.Adapt(entity);
        await _freeSql.Update<IdentityRole>().SetSource(entity).ExecuteAffrowsAsync();
        await _userPermissionManager.DeleteUserRoleCacheByIdAsync(id);
        _preWarmer.PreWarmByRoleId(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        if (await _freeSql.Select<IdentityUserRole>().AnyAsync(x => x.RoleId == id))
        {
            throw new UserFriendlyException("角色已绑定过用户，无法删除");
        }

        await _freeSql.Update<IdentityPage>()
            .Set(x => x.IsDeleted == true)
            .WhereDynamic(id)
            .ExecuteAffrowsAsync();
    }

    /// <summary>
    /// 获取角色已绑定的菜单Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ListResultDto<Guid>> GetMenuIdsAsync(Guid id)
    {
        var selectedIds = await _freeSql.Select<IdentityRoleMenu>()
            .Where(x => x.RoleId == id)
            .ToListAsync(x => x.MenuId);
        return new ListResultDto<Guid>(selectedIds);
    }

    /// <summary>
    /// 更新角色菜单
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <exception cref="UserFriendlyException"></exception>
    public async Task UpdateMenusAsync(Guid id, RoleMenusUpdateDto input)
    {
        if (!await _freeSql.Select<IdentityRole>().AnyAsync(x => x.Id == id))
        {
            throw new UserFriendlyException("编辑失败：角色不存在");
        }
        //删除更新前的菜单、权限关联数据
        using var uow = _freeSql.CreateUnitOfWork();
        await uow.Orm.Delete<IdentityRoleMenu>().Where(x => x.RoleId == id).ExecuteAffrowsAsync();
        var entities = input.Permissions.Select(x => new IdentityRoleMenu
        {
            MenuId = x.Value,        
            RoleId = id
        });
        //添加新的菜单、权限关联数据
        await uow.Orm.Insert(entities).ExecuteAffrowsAsync();
        uow.Commit();
        await _userPermissionManager.DeleteUserRoleCacheByIdAsync(id);
        _preWarmer.PreWarmByRoleId(id);
    }

    public async Task<ListResultDto<GuidSelectDto>> GetSelectAsync()
    {
        var list = await _freeSql.Select<IdentityRole>().ToListAsync(x => new GuidSelectDto
        {
            Value = x.Id,
            Label = x.Name
        });
        return new ListResultDto<GuidSelectDto>(list);
    }
}