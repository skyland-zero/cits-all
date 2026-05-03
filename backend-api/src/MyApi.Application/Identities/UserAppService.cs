using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Cits.IdGenerator;
using Cits.SystemSettings;
using Mapster;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

public class UserAppService : IUserAppService
{
    private readonly IFreeSql _freeSql;
    private readonly IIdGenerator _idGenerator;
    private readonly UserPermissionManager _userPermissionManager;
    private readonly UserPermissionCachePreWarmer _preWarmer;
    private readonly ISettingProvider _settingProvider;
    private readonly OnlineUserSessionManager _onlineUserSessionManager;

    public UserAppService(IFreeSql freeSql, IIdGenerator idGenerator, UserPermissionManager userPermissionManager,
        UserPermissionCachePreWarmer preWarmer, ISettingProvider settingProvider,
        OnlineUserSessionManager onlineUserSessionManager)
    {
        _freeSql = freeSql;
        _idGenerator = idGenerator;
        _userPermissionManager = userPermissionManager;
        _preWarmer = preWarmer;
        _settingProvider = settingProvider;
        _onlineUserSessionManager = onlineUserSessionManager;
    }

    public async Task<UserDto> GetAsync(Guid id)
    {
        var data = await _freeSql.Select<IdentityUser>().WhereDynamic(id).FirstAsync<UserDto>();
        return data;
    }

    public async Task<PagedResultDto<UserDto>> GetListAsync(GetUsersInput input)
    {
        var userName = input.UserName ?? string.Empty;
        var surname = input.Surname ?? string.Empty;

        var query = _freeSql.Select<IdentityUser>()
            .WhereIf(!userName.IsNullOrWhiteSpace(), x => x.UserName.Contains(userName))
            .WhereIf(!surname.IsNullOrWhiteSpace(), x => x.Surname.Contains(surname))
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
        await ValidatePasswordStrengthAsync(input.PasswordHash);
        var entity = input.Adapt<IdentityUser>();
        entity.Id = _idGenerator.Create();
        entity.IsActive = true;
        entity.PasswordHash = PasswordHasher.HashPassword(entity.PasswordHash);
        entity.MustChangePassword = await _settingProvider.GetBoolAsync("security.password.forceChangeInitial");
        entity.PasswordChangedTime = null;
        entity.SecurityStamp = Guid.NewGuid().ToString("N");

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
        var shouldRevokeSessions = entity.IsActive && !input.IsActive;

        input.Adapt(entity);
        await _freeSql.Update<IdentityUser>().SetSource(entity).ExecuteAffrowsAsync();
        //TODO 设置角色
        await _userPermissionManager.DeleteUserPermissionCacheAsync(id);
        if (hasUnitChanged) await _userPermissionManager.DeleteUserOrganizationUnitCacheAsync(id);
        if (shouldRevokeSessions)
        {
            await _onlineUserSessionManager.RevokeUserSessionsAsync(id, "用户已被禁用，会话失效");
        }
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
        await _onlineUserSessionManager.RevokeUserSessionsAsync(id, "用户已删除，会话失效");
    }

    public async Task ResetPasswordAsync(Guid id, UserResetPasswordDto input)
    {
        if (input.Password.IsNullOrWhiteSpace())
        {
            input.Password = "123qwe!@#";
        }

        await ValidatePasswordStrengthAsync(input.Password!);
        var passwordHash = PasswordHasher.HashPassword(input.Password!);
        var forceChangePassword = await _settingProvider.GetBoolAsync("security.password.forceChangeInitial");
        await _freeSql.Update<IdentityUser>()
            .Set(x => x.PasswordHash, passwordHash)
            .Set(x => x.MustChangePassword, forceChangePassword)
            .Set(x => x.PasswordChangedTime, DateTime.UtcNow)
            .Set(x => x.SecurityStamp, Guid.NewGuid().ToString("N"))
            .WhereDynamic(id)
            .ExecuteAffrowsAsync();
        await _onlineUserSessionManager.RevokeUserSessionsAsync(id, "管理员重置密码，会话失效");
        await _userPermissionManager.DeleteUserPermissionCacheAsync(id);
    }

    private async Task ValidatePasswordStrengthAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new UserFriendlyException("密码不能为空");
        }

        var minLength = Math.Max(1, await _settingProvider.GetIntAsync("security.password.minLength", 8));
        var requireUppercase = await _settingProvider.GetBoolAsync("security.password.requireUppercase");
        var requireLowercase = await _settingProvider.GetBoolAsync("security.password.requireLowercase");
        var requireDigit = await _settingProvider.GetBoolAsync("security.password.requireDigit", true);
        var requireNonAlphanumeric = await _settingProvider.GetBoolAsync("security.password.requireNonAlphanumeric");
        var errors = new List<string>();

        if (password.Length < minLength) errors.Add($"至少{minLength}位");
        if (requireUppercase && !password.Any(char.IsUpper)) errors.Add("包含大写字母");
        if (requireLowercase && !password.Any(char.IsLower)) errors.Add("包含小写字母");
        if (requireDigit && !password.Any(char.IsDigit)) errors.Add("包含数字");
        if (requireNonAlphanumeric && password.All(char.IsLetterOrDigit)) errors.Add("包含特殊字符");

        if (errors.Count > 0)
        {
            throw new UserFriendlyException($"密码规则不满足：{string.Join("、", errors)}");
        }
    }
}
