using Microsoft.Extensions.DependencyInjection;

namespace Cits.IdGenerator;

public static class IdGeneratorServiceCollectionExtension
{
    public static IServiceCollection AddIdGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IIdGenerator>(new SequentialIdGenerator());
        return services;
    }
}