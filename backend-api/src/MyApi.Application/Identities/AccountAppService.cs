using System.Security.Claims;
using Cits;
using Cits.IdGenerator;
using Cits.Jwt;
using Cits.LoginLogs;
using Cits.SystemSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

/// <summary>
///     账号登录服务
/// </summary>
public class AccountAppService : IAccountAppService
{
    private readonly IIdGenerator _generator;
    private readonly JwtService _jwtService;
    private readonly UserManager _userManager;
    private readonly OnlineUserSessionManager _onlineUserSessionManager;
    private readonly ILoginLogWriter _loginLogWriter;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;
    private readonly ISettingProvider _settingProvider;
    private readonly UserPermissionManager _userPermissionManager;
    private readonly IFreeSql _freeSql;

    public AccountAppService(IIdGenerator generator, JwtService jwtService, UserManager userManager,
        OnlineUserSessionManager onlineUserSessionManager, ILoginLogWriter loginLogWriter,
        IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ICurrentUser currentUser,
        ISettingProvider settingProvider, UserPermissionManager userPermissionManager, IFreeSql freeSql)
    {
        _generator = generator;
        _jwtService = jwtService;
        _userManager = userManager;
        _onlineUserSessionManager = onlineUserSessionManager;
        _loginLogWriter = loginLogWriter;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _currentUser = currentUser;
        _settingProvider = settingProvider;
        _userPermissionManager = userPermissionManager;
        _freeSql = freeSql;
    }


    /// <summary>
    ///     登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<LoginOutput> LoginAsync(LoginInput input)
    {
        var log = BuildLoginLog(input.UserName);
        var securityOptions = await GetSecurityOptionsAsync();
        var lockResult = await _userManager.IsLockedOutAsync(input.UserName, securityOptions.LockoutMinutes);
        if (lockResult.Item1)
        {
            log.Message = lockResult.Item2;
            await _loginLogWriter.WriteAsync(log);
            throw new UserFriendlyException(lockResult.Item2);
        }

        var user = await _userManager.FindByUserName(input.UserName);
        if (user == null)
        {
            var last = await AccessFailedAsync(input.UserName, securityOptions);
            log.Message = $"用户名或密码错误，剩余尝试次数：{last}";
            await _loginLogWriter.WriteAsync(log);
            throw new UserFriendlyException(BuildLoginFailedMessage(last, securityOptions.LockoutMinutes));
        }

        log.UserId = user.Id.ToString();
        log.RealName = user.Surname;

        if (!PasswordHasher.VerifyPassword(user.PasswordHash, input.Password!))
        {
            var last = await AccessFailedAsync(input.UserName, securityOptions);
            log.Message = $"用户名或密码错误，剩余尝试次数：{last}";
            await _loginLogWriter.WriteAsync(log);
            throw new UserFriendlyException(BuildLoginFailedMessage(last, securityOptions.LockoutMinutes));
        }

        log.Status = true;
        log.Message = "登录成功";
        await _loginLogWriter.WriteAsync(log);
        await _userManager.ResetAccessFailedCountAsync(input.UserName);

        var session = await _onlineUserSessionManager.CreateAsync(
            user,
            GetClientIp(),
            _httpContextAccessor.HttpContext?.Request?.Headers.UserAgent.ToString(),
            GetSessionLifetime());

        return await BuildLoginOutputAsync(user, session.SessionId);
    }

    private LoginLog BuildLoginLog(string username)
    {
        return new LoginLog
        {
            Id = _generator.Create(),
            IP = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"],
            LoginTime = DateTime.Now,
            Status = false,
            UserName = username
        };
    }

    /// <summary>
    ///     退出登录
    /// </summary>
    public async Task<string> LogoutAsync()
    {
        var sessionId = GetCurrentSessionId();
        if (sessionId.HasValue)
        {
            await _onlineUserSessionManager.RevokeAsync(sessionId.Value, "用户主动退出登录");
        }

        return "退出登录成功";
    }

    /// <summary>
    ///     通过刷新token获取访问token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public async Task<LoginOutput> RefreshTokenAsync(string? refreshToken)
    {
        //校验refreshToken
        var (validate, userId, sessionId) = await _jwtService.ValidateRefreshTokenAsync(refreshToken);
        if (!validate || !userId.HasValue || !sessionId.HasValue) throw new UserFriendlyException("refresh token 校验不通过");

        if (!await _onlineUserSessionManager.ValidateAsync(userId.Value, sessionId.Value))
        {
            throw new UserFriendlyException("登录会话已失效，请重新登录");
        }

        var user = await _userManager.FindByUserId(userId.Value);
        if (user == null)
        {
            throw new UserFriendlyException("用户信息校验不通过");
        }

        if (!user.IsActive)
        {
            throw new UserFriendlyException("用户已被禁用");
        }

        return await BuildLoginOutputAsync(user, sessionId.Value);
    }

    [Authorize]
    public async Task ChangePasswordAsync(ChangePasswordDto input)
    {
        if (_currentUser.Id == Guid.Empty)
        {
            throw new UserFriendlyException("请先登录");
        }

        var user = await _userManager.FindByUserId(_currentUser.Id)
                   ?? throw new UserFriendlyException("用户不存在");

        if (!PasswordHasher.VerifyPassword(user.PasswordHash, input.OldPassword))
        {
            throw new UserFriendlyException("旧密码不正确");
        }

        await ValidatePasswordStrengthAsync(input.NewPassword);

        user.PasswordHash = PasswordHasher.HashPassword(input.NewPassword);
        user.MustChangePassword = false;
        user.PasswordChangedTime = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");

        await _freeSql.Update<IdentityUser>()
            .SetSource(user)
            .ExecuteAffrowsAsync();
        await _userPermissionManager.DeleteUserPermissionCacheAsync(user.Id);
        await _onlineUserSessionManager.RevokeUserSessionsExceptAsync(
            user.Id,
            GetCurrentSessionId() ?? Guid.Empty,
            "用户修改密码，旧会话失效");
    }

    private async Task<LoginOutput> BuildLoginOutputAsync(IdentityUser identityUser, Guid sessionId)
    {
        var climes = new List<Claim>
        {
            new(ClaimTypes.Sid, identityUser.Id.ToString()),
            new(ClaimTypes.Name, identityUser.UserName),
            new(ClaimTypes.Surname, identityUser.Surname),
            new(OnlineUserSessionManager.SessionIdClaimType, sessionId.ToString())
        };

        return new LoginOutput
        {
            AccessToken = _jwtService.CreateToken(climes),
            RefreshToken = await _jwtService.CreateRefreshTokenAsync(identityUser.Id, sessionId),
            MustChangePassword = identityUser.MustChangePassword
        };
    }

    private async Task<int> AccessFailedAsync(string username, SecurityOptions securityOptions)
    {
        return await _userManager.AccessFailedAsync(
            username,
            securityOptions.MaxFailedAttempts,
            securityOptions.LockoutMinutes);
    }

    private static string BuildLoginFailedMessage(int last, int lockoutMinutes)
    {
        return $"用户名或密码错误，剩余尝试次数：{last}，之后将锁定账号{lockoutMinutes}分钟。";
    }

    private async Task ValidatePasswordStrengthAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new UserFriendlyException("密码不能为空");
        }

        var options = await GetSecurityOptionsAsync();
        var errors = new List<string>();

        if (password.Length < options.PasswordMinLength)
        {
            errors.Add($"至少{options.PasswordMinLength}位");
        }

        if (options.RequireUppercase && !password.Any(char.IsUpper)) errors.Add("包含大写字母");
        if (options.RequireLowercase && !password.Any(char.IsLower)) errors.Add("包含小写字母");
        if (options.RequireDigit && !password.Any(char.IsDigit)) errors.Add("包含数字");
        if (options.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit)) errors.Add("包含特殊字符");

        if (errors.Count > 0)
        {
            throw new UserFriendlyException($"密码规则不满足：{string.Join("、", errors)}");
        }
    }

    private async Task<SecurityOptions> GetSecurityOptionsAsync()
    {
        return new SecurityOptions(
            PasswordMinLength: Math.Max(1, await _settingProvider.GetIntAsync("security.password.minLength", 8)),
            RequireUppercase: await _settingProvider.GetBoolAsync("security.password.requireUppercase"),
            RequireLowercase: await _settingProvider.GetBoolAsync("security.password.requireLowercase"),
            RequireDigit: await _settingProvider.GetBoolAsync("security.password.requireDigit", true),
            RequireNonAlphanumeric: await _settingProvider.GetBoolAsync("security.password.requireNonAlphanumeric"),
            MaxFailedAttempts: Math.Max(1, await _settingProvider.GetIntAsync("security.login.maxFailedAttempts", 5)),
            LockoutMinutes: Math.Max(1, await _settingProvider.GetIntAsync("security.login.lockoutMinutes", 10)));
    }

    private sealed record SecurityOptions(
        int PasswordMinLength,
        bool RequireUppercase,
        bool RequireLowercase,
        bool RequireDigit,
        bool RequireNonAlphanumeric,
        int MaxFailedAttempts,
        int LockoutMinutes);

    private TimeSpan GetSessionLifetime()
    {
        var hour = _configuration.GetRequiredSection("JWT:RefreshExpireHours").Get<double?>() ?? 48;
        return TimeSpan.FromHours(hour);
    }

    private string? GetClientIp()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
               ?? httpContext?.Connection.RemoteIpAddress?.ToString();
    }

    private Guid? GetCurrentSessionId()
    {
        var value = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == OnlineUserSessionManager.SessionIdClaimType)?.Value;
        return Guid.TryParse(value, out var sessionId) ? sessionId : null;
    }
}
