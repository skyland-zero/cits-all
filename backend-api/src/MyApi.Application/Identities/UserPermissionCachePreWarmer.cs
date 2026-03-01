using Cits.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyApi.Domain.Identities;

namespace MyApi.Application.Identities;

public class UserPermissionCachePreWarmer : ISelfSingletonService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<UserPermissionCachePreWarmer> _logger;

    public UserPermissionCachePreWarmer(IServiceScopeFactory serviceScopeFactory, ILogger<UserPermissionCachePreWarmer> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// 预热指定用户的缓存
    /// </summary>
    public void PreWarmUserCache(Guid userId)
    {
        Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var appService = scope.ServiceProvider.GetRequiredService<IUserPermissionAppService>();
                await appService.PreWarmCacheAsync(userId);
                _logger.LogInformation("Successfully pre-warmed cache for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pre-warming cache for user {UserId}", userId);
            }
        });
    }

    /// <summary>
    /// 预热指定角色下所有用户的缓存
    /// </summary>
    public void PreWarmByRoleId(Guid roleId)
    {
        Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var userPermissionManager = scope.ServiceProvider.GetRequiredService<UserPermissionManager>();
                var userIds = await userPermissionManager.GetUserIdsByRoleIdAsync(roleId);
                var appService = scope.ServiceProvider.GetRequiredService<IUserPermissionAppService>();
                
                foreach (var userId in userIds)
                {
                    await appService.PreWarmCacheAsync(userId);
                }
                _logger.LogInformation("Successfully pre-warmed cache for {Count} users in role {RoleId}", userIds.Count, roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pre-warming cache for role {RoleId}", roleId);
            }
        });
    }

    /// <summary>
    /// 预热所有活跃用户的缓存
    /// </summary>
    public async Task PreWarmAllAsync()
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var userPermissionManager = scope.ServiceProvider.GetRequiredService<UserPermissionManager>();
            var userIds = await userPermissionManager.GetAllUserIdsAsync();
            var appService = scope.ServiceProvider.GetRequiredService<IUserPermissionAppService>();

            foreach (var userId in userIds)
            {
                await appService.PreWarmCacheAsync(userId);
            }
            _logger.LogInformation("Successfully pre-warmed cache for all {Count} active users", userIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pre-warming all users cache");
        }
    }
}
