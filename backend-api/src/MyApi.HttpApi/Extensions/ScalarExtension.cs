using Cits.OpenApi;
using Scalar.AspNetCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ScalarExtension
{
    private const string CacheOpenApiPolicyName = "CacheOpenApi";

    public static IServiceCollection AddScalar(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            options.AddPolicy(CacheOpenApiPolicyName, builder => builder.Expire(TimeSpan.FromDays(30)));
        });
        services.AddOpenApi(opt => { opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
        return services;
    }

    public static IEndpointRouteBuilder MapScalar(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapScalarApiReference(config =>
        {
            config.Title = "MyApi";
            config.DarkMode = false;
            config.ForceThemeMode = ThemeMode.Light;
            config.Theme = ScalarTheme.DeepSpace;
            config.DefaultHttpClient =
                new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.JavaScript, ScalarClient.Axios);
            config.HideModels = true;
            // Bearer
            config.AddHttpAuthentication("BearerAuth", auth =>
                {
                    auth.Token = "ey...";
                });
        }); // scalar/v1
        endpoints.MapOpenApi().CacheOutput(CacheOpenApiPolicyName);

        return endpoints;
    }
}