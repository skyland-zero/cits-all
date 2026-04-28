using Cits.LoginLogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cits.OperationLogs;

public static class OperationLogServiceCollectionExtension
{
    public static IServiceCollection AddOperationLog(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OperationLogOptions>()
            .Bind(configuration.GetSection("OperationLogOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IOperationLogWriter, OperationLogWriter>();
        services.TryAddSingleton<IIpLocationService, IpLocationService>();
        services.AddHostedService<OperationLogService>();
        services.AddHostedService<OperationLogRetentionService>();

        return services;
    }
}