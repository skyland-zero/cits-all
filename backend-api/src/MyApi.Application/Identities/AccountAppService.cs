using System.Security.Claims;
using Cits;
using Cits.IdGenerator;
using Cits.Jwt;
using Cits.LoginLogs;
using Microsoft.AspNetCore.Http;
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
    private readonly ILoginLogWriter _loginLogWriter;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountAppService(IIdGenerator generator, JwtService jwtService, UserManager userManager,
        ILoginLogWriter loginLogWriter, IHttpContextAccessor httpContextAccessor)
    {
        _generator = generator;
        _jwtService = jwtService;
        _userManager = userManager;
        _loginLogWriter = loginLogWriter;
        _httpContextAccessor = httpContextAccessor;
    }


    /// <summary>
    ///     登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task<LoginOutput> LoginAsync(LoginInput input)
    {
        var log = BuildLoginLog(input.UserName);
        var lockResult = await _userManager.IsLockedOutAsync(input.UserName);
        if (lockResult.Item1)
        {
            await _loginLogWriter.WriteAsync(log);
            throw new UserFriendlyException(lockResult.Item2);
        }

        var user = await _userManager.FindByUserName(input.UserName);
        if (user == null)
        {
            var last = await _userManager.AccessFailedAsync(input.UserName);
            await _loginLogWriter.WriteAsync(log);
            throw new UserFriendlyException($"用户名或密码错误，剩余尝试次数：{last}，之后将锁定账号10分钟。");
        }

        log.UserId = user.Id.ToString();
        log.RealName = user.Surname;

        if (!PasswordHasher.VerifyPassword(user.PasswordHash, input.Password!))
        {
            var last = await _userManager.AccessFailedAsync(input.UserName);
            await _loginLogWriter.WriteAsync(log);
            throw new UserFriendlyException($"用户名或密码错误，剩余尝试次数：{last}，之后将锁定账号10分钟。");
        }

        log.Status = true;
        await _loginLogWriter.WriteAsync(log);
        await _userManager.ResetAccessFailedCountAsync(input.UserName);
        return await BuildLoginOutputAsync(user);
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
    public string Logout()
    {
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
        var (validate, userId) = await _jwtService.ValidateRefreshTokenAsync(refreshToken);
        if (!validate || !userId.HasValue) throw new UserFriendlyException("refresh token 校验不通过");

        var user = await _userManager.FindByUserId(userId.Value);
        if (user == null)
        {
            throw new UserFriendlyException("用户信息校验不通过");
        }

        return await BuildLoginOutputAsync(user);
    }

    private async Task<LoginOutput> BuildLoginOutputAsync(IdentityUser identityUser)
    {
        var climes = new List<Claim>
        {
            new(ClaimTypes.Sid, identityUser.Id.ToString()),
            new(ClaimTypes.Name, identityUser.UserName),
            new(ClaimTypes.Surname, identityUser.Surname)
        };

        return new LoginOutput
        {
            AccessToken = _jwtService.CreateToken(climes),
            RefreshToken = await _jwtService.CreateRefreshTokenAsync(identityUser.Id)
        };
    }
}