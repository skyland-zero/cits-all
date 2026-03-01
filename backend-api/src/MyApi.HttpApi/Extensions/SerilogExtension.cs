using Serilog;

namespace MyApi.HttpApi.Extensions;

public static class SerilogExtension
{
    public static IServiceCollection AddSerilogService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        return services;
    }
}