namespace MyApi.HttpApi.Extensions;

using FreeRedis;

public static class FreeRedisExtension
{
    public static void AddFreeRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<RedisClient>(r => new RedisClient(configuration.GetConnectionString("Redis")));
    }
}
