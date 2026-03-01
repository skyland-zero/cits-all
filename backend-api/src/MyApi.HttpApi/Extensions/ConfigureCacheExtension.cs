namespace MyApi.HttpApi.Extensions;

public static class ConfigureCacheExtension
{
    private const bool EnableRedis = true;

    public static IServiceCollection ConfigureHybridCache(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHybridCache();
        if (EnableRedis)
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });

        return services;
    }
}