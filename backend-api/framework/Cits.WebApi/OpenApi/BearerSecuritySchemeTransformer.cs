using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Cits.OpenApi;

/// <summary>
///     https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/openapi/customize-openapi?view=aspnetcore-9.0
/// </summary>
/// <param name="authenticationSchemeProvider"></param>
public sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            // Add the security scheme at the document level
            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer", // "bearer" refers to the header name here
                In = ParameterLocation.Header,
                BearerFormat = "Json Web Token"
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes?.Add("Bearer", scheme);

            // Apply it as a requirement for all operations
            var requirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            };

            foreach (var path in document.Paths.Values)
            {
                if (path.Operations == null) continue;
                foreach (var operation in path.Operations.Values)
                {
                    operation.Security?.Add(requirement);
                }
            }
        }
    }
}