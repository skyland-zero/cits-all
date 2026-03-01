using Microsoft.Extensions.DependencyInjection;

namespace Cits.Jwt;

public static class JwtServiceCollectionExtension
{
    public static IServiceCollection AddJwtService(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IUserContextService, UserContextService>();
        services.AddSingleton<JwtService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }
}