using Cits.ApiResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Cits;

public static class ApiBehaviorConfigureExtension
{
    public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
    {
        //模型绑定 特性验证，自定义返回格式
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value is { Errors.Count: > 0 })
                    .Select(x => new ApiValidationErrorInfo
                    {
                        Member = x.Key,
                        Messages = x.Value == null ? [] : x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    }).ToArray();

                return new BadRequestObjectResult(new ApiErrorResponse(new ApiErrorInfo
                {
                    Code = "ValidationFailed",
                    Message = "参数验证失败",
                    ValidationErrors = errors
                }));
            };
        });

        return services;
    }
}