using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyApi.Application.Identities;

public class UserPermissionPreWarmBackgroundService : BackgroundService
{
    private readonly UserPermissionCachePreWarmer _preWarmer;
    private readonly ILogger<UserPermissionPreWarmBackgroundService> _logger;

    public UserPermissionPreWarmBackgroundService(UserPermissionCachePreWarmer preWarmer, ILogger<UserPermissionPreWarmBackgroundService> logger)
    {
        _preWarmer = preWarmer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting user permission cache pre-warming...");
        
        // Wait a bit for the application to be fully ready
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        
        if (stoppingToken.IsCancellationRequested) return;

        await _preWarmer.PreWarmAllAsync();
        
        _logger.LogInformation("User permission cache pre-warming completed.");
    }
}
