using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyApi.Domain.DomainServices.CorpWx;

public static class CropWxServiceCollectionExtension
{
    public static IServiceCollection AddCorpWxService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<CorpWxOptions>(configuration.GetSection("CorpWxOptions"));
        services.AddSingleton<ICorpWxClient, CorpWxClient>();
        return services;
    }
}
