using Cits.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cits.Permissions;

public static class PermissionConfigureExtension
{
    public static IServiceCollection AddPermission(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PermissionOptions>(configuration.GetSection("Permission"));
        services.AddSingleton<IAuthorizationPolicyProvider, CitsPermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, CitsPermissionAuthorizationHandler>();
        services.AddScoped<PermissionManager>();

        services.AddHostedService<PermissionInitializeBackgroundService>();

        return services;
    }
}