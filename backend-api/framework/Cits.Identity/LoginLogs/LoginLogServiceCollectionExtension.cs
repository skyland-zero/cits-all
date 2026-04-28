using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;

namespace Cits.LoginLogs;

public static class LoginLogServiceCollectionExtension
{
    public static IServiceCollection AddLoginLog(this IServiceCollection services)
    {
        services.AddSingleton<ILoginLogWriter, LoginLogWriter>();
        services.AddSingleton<IIpLocationService, IpLocationService>();

        services.AddHostedService<LoginLogService>();
        // services.AddHostedService<LoginLogUaParseService>();

        services.AddHttpUserAgentParser();

        services.AddHttpUserAgentMemoryCachedParser();

        return services;
    }
}