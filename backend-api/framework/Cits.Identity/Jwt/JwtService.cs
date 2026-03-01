using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cits.Extensions;
using Cits.IdGenerator;
using Cits.Jwt.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cits.Jwt;

public class JwtService
{
    private readonly HybridCache _cache;
    private readonly IConfiguration _configuration;
    private readonly IIdGenerator _idGenerator;

    public JwtService(IConfiguration configuration, HybridCache cache, IIdGenerator idGenerator)
    {
        _configuration = configuration;
        _cache = cache;
        _idGenerator = idGenerator;
    }


    /// <summary>
    ///     生成访问Token
    /// </summary>
    /// <param name="claims"></param>
    /// <returns></returns>
    public string CreateToken(IEnumerable<Claim> claims)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? string.Empty));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwtSettings["Issuer"],
            jwtSettings["Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryHours"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    /// <summary>
    ///     生成刷新token
    /// </summary>
    /// <returns></returns>
    public async Task<string> CreateRefreshTokenAsync(Guid userId)
    {
        var token = _idGenerator.Create().ToString();
        var key = GetHashedKey(token);
        var hour = _configuration.GetRequiredSection("JWT:RefreshExpireHours").Get<double?>() ?? 48;
        var expire = TimeSpan.FromHours(hour);
        await _cache.SetAsync(key, new RefreshTokenCacheModel(userId, expire)
            , new HybridCacheEntryOptions
            {
                Expiration = expire
            });
        return token;
    }

    /// <summary>
    ///     校验刷新token是否有效
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<(bool, Guid?)> ValidateRefreshTokenAsync(string? token)
    {
        if (token == null)
        {
            return (false, null);
        }
        var key = GetHashedKey(token);
        var cache = await _cache.GetOrCreateAsync<RefreshTokenCacheModel?>(key,
            _ => new ValueTask<RefreshTokenCacheModel?>(),
            new HybridCacheEntryOptions
            {
                Flags = HybridCacheEntryFlags.DisableLocalCacheWrite |
                        HybridCacheEntryFlags.DisableDistributedCacheWrite
            });
        if (cache == null || cache.UserId == Guid.Empty) return (false, null);

        //刷新token只能生效一次，判断过后即删除对应缓存
        await _cache.RemoveAsync(key);
        var effective = cache.Expire >= TimeSpan.Zero;
        return (effective, cache.UserId);
    }

    private string GetHashedKey(string value)
    {
        return value.ToSha256();
    }
}