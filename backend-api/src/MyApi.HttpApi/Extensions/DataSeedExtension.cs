using MyApi.Domain;

namespace MyApi.HttpApi.Extensions;

public static class DataSeedExtension
{
    public static IServiceCollection ConfigureDataSeed(this IServiceCollection services)
    {
        services.AddHostedService<SystemDataSeedContributor>();

        return services;
    }
}