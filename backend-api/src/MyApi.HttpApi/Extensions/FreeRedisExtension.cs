namespace MyApi.HttpApi.Extensions;

using FreeRedis;

public static class FreeRedisExtension
{
    public static void AddFreeRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var useLocal = configuration.GetValue<bool>("DbConfig:UseLocal");
        var connectionString = configuration.GetConnectionString(useLocal ? "LocalRedis" : "RemoteRedis");
        services.AddSingleton<RedisClient>(r => new RedisClient(connectionString));
    }
}

