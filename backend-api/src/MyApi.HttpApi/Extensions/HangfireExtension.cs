using Hangfire;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MyApi.HttpApi.Extensions;

public static class HangfireExtension
{
    public static void AddHangfireService(this IServiceCollection services, IConfiguration configuration)
    {
        var useLocal = configuration.GetValue<bool>("DbConfig:UseLocal");
        
        services.AddHangfire(config => 
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings();

            if (useLocal)
            {
                config.UseInMemoryStorage();
            }
            else
            {
                var connectionString = configuration.GetConnectionString("RemoteRedis");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new System.Exception("RemoteRedis connection string is not configured.");
                }
                var options = ConfigurationOptions.Parse(connectionString);
                var redis = ConnectionMultiplexer.Connect(options);
                config.UseRedisStorage(redis);
            }
        });

        services.AddHangfireServer();
    }
}
