using Cits;
using Cits.DI;
using Microsoft.Extensions.Caching.Hybrid;

namespace MyApi.Domain.Identities;

public class OnlineUserSessionManager : ISelfSingletonService
{
    public const string SessionIdClaimType = "session_id";
    private const string SessionCacheKeyPrefix = "online_session:";
    private const string SessionTouchCacheKeyPrefix = "online_session_touch:";
    private static readonly TimeSpan TouchThrottle = TimeSpan.FromSeconds(45);
    private readonly IFreeSql _freeSql;
    private readonly HybridCache _hybridCache;

    public OnlineUserSessionManager(IFreeSql freeSql, HybridCache hybridCache)
    {
        _freeSql = freeSql;
        _hybridCache = hybridCache;
    }

    public async Task<IdentityOnlineUserSession> CreateAsync(
        IdentityUser user,
        string? ip,
        string? userAgent,
        TimeSpan lifetime,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entity = new IdentityOnlineUserSession
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            UserId = user.Id,
            UserName = user.UserName,
            Surname = user.Surname,
            Ip = Truncate(ip, 80),
            UserAgent = Truncate(userAgent, 500),
            LoginTime = now,
            LastActiveTime = now,
            ExpireTime = now.Add(lifetime)
        };

        await _freeSql.Insert(entity).ExecuteAffrowsAsync(cancellationToken);
        await RemoveSessionCacheAsync(entity.SessionId, cancellationToken);
        return entity;
    }

    public async Task<bool> ValidateAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await GetSessionSnapshotAsync(sessionId, cancellationToken);
        if (session == null) return false;
        if (session.UserId != userId) return false;
        if (session.IsRevoked) return false;
        if (session.ExpireTime <= DateTime.UtcNow) return false;

        await TouchAsync(sessionId, cancellationToken);
        return true;
    }

    public async Task RevokeAsync(Guid sessionId, string reason, CancellationToken cancellationToken = default)
    {
        await _freeSql.Update<IdentityOnlineUserSession>()
            .Set(x => x.IsRevoked, true)
            .Set(x => x.RevokedTime, DateTime.UtcNow)
            .Set(x => x.RevokedReason, reason)
            .Where(x => x.SessionId == sessionId && !x.IsRevoked)
            .ExecuteAffrowsAsync(cancellationToken);

        await RemoveSessionCacheAsync(sessionId, cancellationToken);
    }

    public async Task RevokeUserSessionsAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        var sessionIds = await _freeSql.Select<IdentityOnlineUserSession>()
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync(x => x.SessionId, cancellationToken);

        if (sessionIds.Count == 0) return;

        await _freeSql.Update<IdentityOnlineUserSession>()
            .Set(x => x.IsRevoked, true)
            .Set(x => x.RevokedTime, DateTime.UtcNow)
            .Set(x => x.RevokedReason, reason)
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ExecuteAffrowsAsync(cancellationToken);

        foreach (var sessionId in sessionIds)
        {
            await RemoveSessionCacheAsync(sessionId, cancellationToken);
        }
    }

    public async Task RevokeUserSessionsExceptAsync(
        Guid userId,
        Guid exceptSessionId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var sessionIds = await _freeSql.Select<IdentityOnlineUserSession>()
            .Where(x => x.UserId == userId && x.SessionId != exceptSessionId && !x.IsRevoked)
            .ToListAsync(x => x.SessionId, cancellationToken);

        if (sessionIds.Count == 0) return;

        await _freeSql.Update<IdentityOnlineUserSession>()
            .Set(x => x.IsRevoked, true)
            .Set(x => x.RevokedTime, DateTime.UtcNow)
            .Set(x => x.RevokedReason, reason)
            .Where(x => x.UserId == userId && x.SessionId != exceptSessionId && !x.IsRevoked)
            .ExecuteAffrowsAsync(cancellationToken);

        foreach (var sessionId in sessionIds)
        {
            await RemoveSessionCacheAsync(sessionId, cancellationToken);
        }
    }

    private async Task TouchAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var touchKey = $"{SessionTouchCacheKeyPrefix}{sessionId:N}";
        var shouldTouch = await _hybridCache.GetOrCreateAsync(
            touchKey,
            _ => ValueTask.FromResult(true),
            new HybridCacheEntryOptions
            {
                Expiration = TouchThrottle,
                LocalCacheExpiration = TouchThrottle
            },
            cancellationToken: cancellationToken);

        if (!shouldTouch) return;

        await _hybridCache.SetAsync(
            touchKey,
            false,
            new HybridCacheEntryOptions
            {
                Expiration = TouchThrottle,
                LocalCacheExpiration = TouchThrottle
            },
            cancellationToken: cancellationToken);

        await _freeSql.Update<IdentityOnlineUserSession>()
            .Set(x => x.LastActiveTime, DateTime.UtcNow)
            .Where(x => x.SessionId == sessionId && !x.IsRevoked)
            .ExecuteAffrowsAsync(cancellationToken);

        await RemoveSessionCacheAsync(sessionId, cancellationToken);
    }

    private async Task<OnlineUserSessionSnapshot?> GetSessionSnapshotAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await _hybridCache.GetOrCreateAsync(
            BuildSessionCacheKey(sessionId),
            async _ => await _freeSql.Select<IdentityOnlineUserSession>()
                .Where(x => x.SessionId == sessionId)
                .FirstAsync(x => new OnlineUserSessionSnapshot
                {
                    UserId = x.UserId,
                    IsRevoked = x.IsRevoked,
                    ExpireTime = x.ExpireTime
                }, cancellationToken),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(30),
                LocalCacheExpiration = TimeSpan.FromSeconds(30)
            },
            cancellationToken: cancellationToken);
    }

    private async Task RemoveSessionCacheAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        await _hybridCache.RemoveAsync(BuildSessionCacheKey(sessionId), cancellationToken);
    }

    private static string BuildSessionCacheKey(Guid sessionId) => $"{SessionCacheKeyPrefix}{sessionId:N}";

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private sealed class OnlineUserSessionSnapshot
    {
        public Guid UserId { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime ExpireTime { get; set; }
    }
}
