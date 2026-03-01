using Cits.ApiResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Cits;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UserFriendlyException userFriendlyException)
        {
            context.Result = new JsonResult(new ApiErrorResponse(new ApiErrorInfo
            {
                Message = userFriendlyException.Message,
                Code = userFriendlyException.Code
            }))
            {
                StatusCode = 400 // Bad Request
            };
        }
        else
        {
            // 其他异常返回 500
            context.Result = new JsonResult(new ApiErrorResponse(new ApiErrorInfo
            {
                Message = "服务器内部错误",
                Code = "INTERNAL_SERVER_ERROR"
            }))
            {
                StatusCode = 500
            };

            _logger.LogError(context.Exception, context.Exception.Message);
        }

        context.ExceptionHandled = true;
    }
}