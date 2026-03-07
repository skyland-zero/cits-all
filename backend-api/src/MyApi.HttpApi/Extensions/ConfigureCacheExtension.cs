namespace MyApi.HttpApi.Extensions;

public static class ConfigureCacheExtension
{
    private const bool EnableRedis = true;

    public static IServiceCollection ConfigureHybridCache(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHybridCache();
        var useLocal = configuration.GetValue<bool>("DbConfig:UseLocal");

        if (useLocal)
        {
            services.AddDistributedMemoryCache();
        }
        else if (EnableRedis)
        {
            var connectionString = configuration.GetConnectionString("RemoteRedis");
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionString;
            });
        }

        return services;
    }
}