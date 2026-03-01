using System.Text.RegularExpressions;
using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Cits.IdGenerator;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

public class UserAppService : IUserAppService
{
    private readonly IFreeSql _freeSql;
    private readonly IIdGenerator _idGenerator;
    private readonly UserPermissionManager _userPermissionManager;
    private readonly UserPermissionCachePreWarmer _preWarmer;

    public UserAppService(IFreeSql freeSql, IIdGenerator idGenerator, UserPermissionManager userPermissionManager, UserPermissionCachePreWarmer preWarmer)
    {
        _freeSql = freeSql;
        _idGenerator = idGenerator;
        _userPermissionManager = userPermissionManager;
        _preWarmer = preWarmer;
    }

    public async Task<UserDto> GetAsync(Guid id)
    {
        var data = await _freeSql.Select<IdentityUser>().WhereDynamic(id).FirstAsync<UserDto>();
        return data;
    }

    public async Task<PagedResultDto<UserDto>> GetListAsync(GetUsersInput input)
    {
        var query = _freeSql.Select<IdentityUser>()
            .WhereIf(!input.UserName.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.UserName))
            .WhereIf(!input.Surname.IsNullOrWhiteSpace(), x => x.Surname.Contains(input.Surname))
            .WhereIf(input.OrganizationUnitId.HasValue, x => x.OrganizationUnitId == input.OrganizationUnitId);
        var count = await query.CountAsync();
        if (count == 0)
        {
            return new PagedResultDto<UserDto>();
        }

        query = query.OrderByDescending(x => x.CreationTime).PageBy(input);
        var list = await query.ToListAsync<UserDto>();
        return new PagedResultDto<UserDto>(count, list);
    }

    public async Task CreateAsync(UserCreateDto input)
    {
        ValidatePasswordStrength(input.PasswordHash);
        var entity = input.Adapt<IdentityUser>();
        entity.Id = _idGenerator.Create();
        entity.IsActive = true;
        entity.PasswordHash = PasswordHasher.HashPassword(entity.PasswordHash);

        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
    }

    public async Task UpdateAsync(Guid id, UserUpdateDto input)
    {
        var entity = await _freeSql.Select<IdentityUser>().WhereDynamic(id).FirstAsync();
        if (entity == null)
        {
            throw new UserFriendlyException("数据不存在");
        }

        var hasUnitChanged = HasOrganizationUnitChanged(input, entity);

        input.Adapt(entity);
        await _freeSql.Update<IdentityUser>().SetSource(entity).ExecuteAffrowsAsync();
        //TODO 设置角色
        await _userPermissionManager.DeleteUserPermissionCacheAsync(id);
        if (hasUnitChanged) await _userPermissionManager.DeleteUserOrganizationUnitCacheAsync(id);
        _preWarmer.PreWarmUserCache(id);
    }

    private static bool HasOrganizationUnitChanged(UserUpdateDto a, IdentityUser b)
    {
        return a.OrganizationUnitId != b.OrganizationUnitId;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _freeSql.Update<IdentityUser>()
            .Set(x => x.IsDeleted == true)
            .WhereDynamic(id)
            .ExecuteAffrowsAsync();
    }

    public async Task ResetPasswordAsync(Guid id, UserResetPasswordDto input)
    {
        if (input.Password.IsNullOrWhiteSpace())
        {
            input.Password = "123qwe!@#";
        }

        ValidatePasswordStrength(input.Password!);
        var passwordHash = PasswordHasher.HashPassword(input.Password!);
        await _freeSql.Update<IdentityUser>()
            .Set(x => x.PasswordHash, passwordHash)
            .WhereDynamic(id)
            .ExecuteAffrowsAsync();
    }

    private static void ValidatePasswordStrength(string password)
    {
        bool isValid = Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[\s\S]{8,}$");
        if (!isValid)
        {
            throw new UserFriendlyException("密码至少8位，包含大小写字母和数字");
        }
    }
}