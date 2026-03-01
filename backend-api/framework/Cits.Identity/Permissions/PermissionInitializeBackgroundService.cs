using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cits.Permissions;

public class PermissionInitializeBackgroundService : BackgroundService
{
    private readonly ILogger<PermissionInitializeBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PermissionInitializeBackgroundService(ILogger<PermissionInitializeBackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("初始化权限数据开始");
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        var pm = scope.ServiceProvider.GetRequiredService<PermissionManager>();
        await pm.SyncPermissionsAsync(scope.ServiceProvider);
        _logger.LogInformation("初始化权限数据完成");
    }
}