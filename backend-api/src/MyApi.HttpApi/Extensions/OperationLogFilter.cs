using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Cits;
using Cits.ApiResponse;
using Cits.OperationLogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using MyApi.Domain.Identities;

namespace MyApi.HttpApi.Extensions;

public class OperationLogFilter : IAsyncActionFilter
{
    private const int MaxSanitizeDepth = 4;
    private const int MaxCollectionItems = 100;

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "oldPassword",
        "newPassword",
        "confirmPassword",
        "token",
        "refreshToken",
        "authorization",
        "accessToken"
    };

    private readonly ICurrentUser _currentUser;
    private readonly UserPermissionManager _userPermissionManager;
    private readonly IOperationLogWriter _operationLogWriter;
    private readonly IOptions<JsonOptions> _jsonOptions;
    private readonly OperationLogOptions _operationLogOptions;
    private readonly ILogger<OperationLogFilter> _logger;

    public OperationLogFilter(
        ICurrentUser currentUser,
        UserPermissionManager userPermissionManager,
        IOperationLogWriter operationLogWriter,
        IOptions<JsonOptions> jsonOptions,
        IOptions<OperationLogOptions> operationLogOptions,
        ILogger<OperationLogFilter> logger)
    {
        _currentUser = currentUser;
        _userPermissionManager = userPermissionManager;
        _operationLogWriter = operationLogWriter;
        _jsonOptions = jsonOptions;
        _operationLogOptions = operationLogOptions.Value;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
        {
            await next();
            return;
        }

        var controllerAttribute = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<OperationLogAttribute>(true);
        var actionAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttribute<OperationLogAttribute>(true);
        var skipAttribute = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttribute<SkipOperationLogAttribute>(true) != null
            || controllerActionDescriptor.MethodInfo.GetCustomAttribute<SkipOperationLogAttribute>(true) != null;

        if (!ShouldLog(context, actionAttribute, skipAttribute))
        {
            await next();
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var request = context.HttpContext.Request;
        var operationTime = DateTime.Now;
        var requestParameters = BuildRequestParametersSafely(context, controllerActionDescriptor);
        var departmentPath = await GetDepartmentPathAsync();

        var executedContext = await next();

        stopwatch.Stop();

        try
        {
            var result = executedContext.Result;
            var statusCode = ResolveStatusCode(result, context.HttpContext.Response.StatusCode);
            var operationLog = new OperationLog
            {
                Module = ResolveModule(controllerActionDescriptor, controllerAttribute, actionAttribute),
                OperationType = ResolveOperationType(request.Method, controllerActionDescriptor, controllerAttribute, actionAttribute),
                OperatorId = _currentUser.IsAuthenticated ? _currentUser.Id.ToString() : null,
                OperatorName = ResolveOperatorName(),
                DepartmentPath = departmentPath,
                OperationIp = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                Status = ResolveSuccess(executedContext, result, statusCode),
                OperationTime = operationTime,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                RequestPath = request.Path.Value,
                RequestMethod = request.Method,
                RequestParameters = requestParameters,
                ResponseParameters = BuildResponseParametersSafely(result, statusCode),
                ErrorMessage = BuildErrorMessageSafely(executedContext, result)
            };

            await _operationLogWriter.WriteAsync(operationLog);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "记录操作日志失败: {Path}", request.Path.Value);
        }
    }

    private string? BuildRequestParametersSafely(ActionExecutingContext context, ControllerActionDescriptor controllerActionDescriptor)
    {
        try
        {
            return SerializeAndTruncate(BuildRequestPayload(context, controllerActionDescriptor), _operationLogOptions.MaxRequestLength);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "采集操作日志请求参数失败: {Path}", context.HttpContext.Request.Path.Value);
            return SerializeAndTruncate(new Dictionary<string, object?>
            {
                ["type"] = "capture_error",
                ["message"] = ex.Message
            }, _operationLogOptions.MaxRequestLength);
        }
    }

    private string? BuildResponseParametersSafely(IActionResult? result, int statusCode)
    {
        try
        {
            return SerializeAndTruncate(BuildResponsePayload(result, statusCode), _operationLogOptions.MaxResponseLength);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "采集操作日志响应参数失败");
            return SerializeAndTruncate(new Dictionary<string, object?>
            {
                ["type"] = "capture_error",
                ["message"] = ex.Message
            }, _operationLogOptions.MaxResponseLength);
        }
    }

    private string? BuildErrorMessageSafely(ActionExecutedContext executedContext, IActionResult? result)
    {
        try
        {
            return Truncate(SanitizeText(BuildErrorMessage(executedContext, result)), _operationLogOptions.MaxErrorLength);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "采集操作日志错误信息失败");
            return Truncate(ex.Message, _operationLogOptions.MaxErrorLength);
        }
    }

    private bool ShouldLog(
        ActionExecutingContext context,
        OperationLogAttribute? actionAttribute,
        bool skipAttribute)
    {
        if (skipAttribute)
        {
            return false;
        }

        var method = context.HttpContext.Request.Method;
        if (HttpMethods.IsOptions(method) || HttpMethods.IsHead(method))
        {
            return false;
        }

        if (HttpMethods.IsGet(method))
        {
            return actionAttribute is not null;
        }

        return true;
    }

    private async Task<string?> GetDepartmentPathAsync()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return null;
        }

        try
        {
            var organizationUnit = await _userPermissionManager.GetUserOrganizationUnitAsync(_currentUser.Id);
            if (organizationUnit is null)
            {
                return null;
            }

            return string.IsNullOrWhiteSpace(organizationUnit.NamePath)
                ? string.IsNullOrWhiteSpace(organizationUnit.Name) ? null : organizationUnit.Name
                : organizationUnit.NamePath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取用户部门路径失败: {UserId}", _currentUser.Id);
            return null;
        }
    }

    private string? ResolveOperatorName()
    {
        if (!_currentUser.IsAuthenticated)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_currentUser.Surname))
        {
            return _currentUser.Surname;
        }

        return string.IsNullOrWhiteSpace(_currentUser.UserName) ? null : _currentUser.UserName;
    }

    private string ResolveModule(
        ControllerActionDescriptor controllerActionDescriptor,
        OperationLogAttribute? controllerAttribute,
        OperationLogAttribute? actionAttribute)
    {
        return actionAttribute?.Module
               ?? controllerAttribute?.Module
               ?? controllerActionDescriptor.ControllerName;
    }

    private string ResolveOperationType(
        string requestMethod,
        ControllerActionDescriptor controllerActionDescriptor,
        OperationLogAttribute? controllerAttribute,
        OperationLogAttribute? actionAttribute)
    {
        return actionAttribute?.OperationType
               ?? controllerAttribute?.OperationType
               ?? InferOperationType(requestMethod, controllerActionDescriptor.ActionName);
    }

    private static string InferOperationType(string requestMethod, string actionName)
    {
        var normalizedActionName = actionName.ToLowerInvariant();

        if (normalizedActionName.Contains("getlist") || normalizedActionName.Contains("list") || normalizedActionName.Contains("tree") || normalizedActionName.Contains("select") || normalizedActionName.Contains("stats") || normalizedActionName.Contains("current") || normalizedActionName.Contains("menus"))
        {
            return OperationLogActions.List;
        }

        if (normalizedActionName.StartsWith("get") || normalizedActionName.Contains("detail"))
        {
            return OperationLogActions.Detail;
        }

        if (normalizedActionName.Contains("create") || normalizedActionName.Contains("add"))
        {
            return OperationLogActions.Create;
        }

        if (normalizedActionName.Contains("update") || normalizedActionName.Contains("edit"))
        {
            return OperationLogActions.Edit;
        }

        if (normalizedActionName.Contains("delete") || normalizedActionName.Contains("remove"))
        {
            return OperationLogActions.Delete;
        }

        if (normalizedActionName.Contains("resetpassword"))
        {
            return OperationLogActions.ResetPassword;
        }

        if (HttpMethods.IsGet(requestMethod))
        {
            return OperationLogActions.Detail;
        }

        if (HttpMethods.IsDelete(requestMethod))
        {
            return OperationLogActions.Delete;
        }

        return OperationLogActions.Edit;
    }

    private object BuildRequestPayload(ActionExecutingContext context, ControllerActionDescriptor controllerActionDescriptor)
    {
        var actionArguments = new Dictionary<string, object?>();
        var serviceParameterNames = controllerActionDescriptor.MethodInfo.GetParameters()
            .Where(IsServiceParameter)
            .Select(x => x.Name)
            .OfType<string>()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in context.ActionArguments)
        {
            if (serviceParameterNames.Contains(key))
            {
                continue;
            }

            var sanitized = SanitizeValue(value, key, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
            if (sanitized is not null)
            {
                actionArguments[key] = sanitized;
            }
        }

        var query = new Dictionary<string, object?>();
        foreach (var item in context.HttpContext.Request.Query)
        {
            object? value = item.Value.Count switch
            {
                0 => null,
                1 => item.Value[0],
                _ => item.Value.ToArray()
            };
            query[item.Key] = SanitizeValue(value, item.Key, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }

        var route = new Dictionary<string, object?>();
        foreach (var item in context.RouteData.Values)
        {
            if (item.Key.Equals("controller", StringComparison.OrdinalIgnoreCase)
                || item.Key.Equals("action", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            route[item.Key] = SanitizeValue(item.Value?.ToString(), item.Key, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
        }

        return new Dictionary<string, object?>
        {
            ["route"] = route,
            ["query"] = query,
            ["arguments"] = actionArguments
        };
    }

    private object? BuildResponsePayload(IActionResult? result, int statusCode)
    {
        return result switch
        {
            ObjectResult objectResult => SanitizeValue(objectResult.Value, null, 0, new HashSet<object>(ReferenceEqualityComparer.Instance)),
            JsonResult jsonResult => SanitizeValue(jsonResult.Value, null, 0, new HashSet<object>(ReferenceEqualityComparer.Instance)),
            ContentResult contentResult => new Dictionary<string, object?>
            {
                ["type"] = "content",
                ["statusCode"] = ResolveStatusCode(contentResult, statusCode),
                ["length"] = contentResult.Content?.Length ?? 0
            },
            FileResult fileResult => new Dictionary<string, object?>
            {
                ["type"] = "file",
                ["statusCode"] = ResolveStatusCode(fileResult, statusCode),
                ["contentType"] = fileResult.ContentType,
                ["fileDownloadName"] = fileResult.FileDownloadName
            },
            StatusCodeResult statusCodeResult => new Dictionary<string, object?>
            {
                ["type"] = "statusCode",
                ["statusCode"] = ResolveStatusCode(statusCodeResult, statusCode)
            },
            EmptyResult => new Dictionary<string, object?>
            {
                ["type"] = "empty",
                ["statusCode"] = statusCode
            },
            null => new Dictionary<string, object?>
            {
                ["type"] = "none",
                ["statusCode"] = statusCode
            },
            _ => new Dictionary<string, object?>
            {
                ["type"] = result.GetType().Name,
                ["statusCode"] = statusCode
            }
        };
    }

    private bool ResolveSuccess(ActionExecutedContext executedContext, IActionResult? result, int statusCode)
    {
        if (executedContext.Exception is not null)
        {
            return false;
        }

        if (statusCode >= 400)
        {
            return false;
        }

        return ExtractApiErrorResponse(result) is null;
    }

    private static int ResolveStatusCode(IActionResult? result, int fallbackStatusCode)
    {
        if (result is IStatusCodeActionResult statusCodeActionResult && statusCodeActionResult.StatusCode.HasValue)
        {
            return statusCodeActionResult.StatusCode.Value;
        }

        return fallbackStatusCode == 0 ? StatusCodes.Status200OK : fallbackStatusCode;
    }

    private string? BuildErrorMessage(ActionExecutedContext executedContext, IActionResult? result)
    {
        if (executedContext.Exception is not null)
        {
            return executedContext.Exception.Message;
        }

        var apiError = ExtractApiErrorResponse(result);
        if (!string.IsNullOrWhiteSpace(apiError?.Error?.Message))
        {
            return apiError.Error.Message;
        }

        return result switch
        {
            ObjectResult { Value: string message } => message,
            JsonResult { Value: string message } => message,
            ContentResult contentResult when !string.IsNullOrWhiteSpace(contentResult.Content) => contentResult.Content,
            _ => null
        };
    }

    private static ApiErrorResponse? ExtractApiErrorResponse(IActionResult? result)
    {
        return result switch
        {
            ObjectResult { Value: ApiErrorResponse response } => response,
            JsonResult { Value: ApiErrorResponse response } => response,
            _ => null
        };
    }

    private string? SerializeAndTruncate(object? value, int maxLength)
    {
        if (value is null)
        {
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions.Value.JsonSerializerOptions);
            return Truncate(json, maxLength);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "序列化操作日志内容失败");
            var fallback = JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["type"] = "serialization_error",
                ["message"] = ex.Message
            });
            return Truncate(fallback, maxLength);
        }
    }

    private object? SanitizeValue(object? value, string? name, int depth, HashSet<object> visited)
    {
        if (IsSensitiveKey(name))
        {
            return "***";
        }

        if (value is null)
        {
            return null;
        }

        if (value is string stringValue)
        {
            return stringValue;
        }

        var type = value.GetType();

        if (type.IsPrimitive || type.IsEnum || value is decimal || value is Guid || value is DateTime || value is DateTimeOffset || value is TimeSpan)
        {
            return value;
        }

        if (value is IFormFile formFile)
        {
            return new Dictionary<string, object?>
            {
                ["type"] = "file",
                ["name"] = formFile.FileName,
                ["contentType"] = formFile.ContentType,
                ["length"] = formFile.Length
            };
        }

        if (value is IFormFileCollection formFiles)
        {
            return formFiles.Select(file => new Dictionary<string, object?>
            {
                ["type"] = "file",
                ["name"] = file.FileName,
                ["contentType"] = file.ContentType,
                ["length"] = file.Length
            }).ToList();
        }

        if (value is Stream)
        {
            return new Dictionary<string, object?> { ["type"] = "stream" };
        }

        if (value is byte[] bytes)
        {
            return new Dictionary<string, object?>
            {
                ["type"] = "bytes",
                ["length"] = bytes.Length
            };
        }

        if (value is HttpContext or ClaimsPrincipal or CancellationToken or IServiceProvider)
        {
            return null;
        }

        if (depth >= MaxSanitizeDepth)
        {
            return "[max_depth]";
        }

        if (!type.IsValueType && !visited.Add(value))
        {
            return "[circular]";
        }

        if (value is IDictionary dictionary)
        {
            var result = new Dictionary<string, object?>();
            var count = 0;
            foreach (DictionaryEntry entry in dictionary)
            {
                if (count >= MaxCollectionItems)
                {
                    result["__truncated"] = true;
                    break;
                }

                var key = entry.Key?.ToString() ?? string.Empty;
                result[key] = SanitizeValue(entry.Value, key, depth + 1, visited);
                count++;
            }
            return result;
        }

        if (value is IEnumerable enumerable)
        {
            var list = new List<object?>();
            var count = 0;
            foreach (var item in enumerable)
            {
                if (count >= MaxCollectionItems)
                {
                    list.Add("[truncated_items]");
                    break;
                }

                list.Add(SanitizeValue(item, null, depth + 1, visited));
                count++;
            }
            return list;
        }

        var payload = new Dictionary<string, object?>();
        foreach (var property in GetSerializableProperties(type))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0 || ShouldSkipType(property.PropertyType))
            {
                continue;
            }

            try
            {
                payload[property.Name] = SanitizeValue(property.GetValue(value), property.Name, depth + 1, visited);
            }
            catch
            {
                payload[property.Name] = null;
            }
        }

        return payload;
    }

    private static PropertyInfo[] GetSerializableProperties(Type type)
    {
        return PropertyCache.GetOrAdd(type, static item => item
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead
                               && property.GetIndexParameters().Length == 0
                               && !ShouldSkipType(property.PropertyType))
            .ToArray());
    }

    private static string? SanitizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var sanitized = value;
        foreach (var key in SensitiveKeys)
        {
            sanitized = Regex.Replace(
                sanitized,
                $"({Regex.Escape(key)}\\s*[:=]\\s*)[^\\s,;}}]+",
                "$1***",
                RegexOptions.IgnoreCase);
        }

        return sanitized;
    }

    private static bool ShouldSkipType(Type type)
    {
        return typeof(HttpContext).IsAssignableFrom(type)
               || typeof(ClaimsPrincipal).IsAssignableFrom(type)
               || typeof(CancellationToken) == type
               || typeof(IServiceProvider).IsAssignableFrom(type)
               || typeof(Stream).IsAssignableFrom(type)
               || typeof(Delegate).IsAssignableFrom(type);
    }

    private static bool IsServiceParameter(ParameterInfo parameterInfo)
    {
        return parameterInfo.GetCustomAttribute<FromServicesAttribute>() is not null;
    }

    private static bool IsSensitiveKey(string? key)
    {
        return !string.IsNullOrWhiteSpace(key) && SensitiveKeys.Contains(key);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..maxLength]}...(truncated)";
    }
}