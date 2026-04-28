using System.Security.Claims;
using FreeRedis;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MyApi.HttpApi.Extensions;

/// <summary>
/// 在线用户过滤器 - 记录用户最后活跃时间
/// </summary>
public class OnlineUserFilter : IAsyncActionFilter
{
    private readonly RedisClient _redis;
    private const string OnlineUsersKey = "sys:online_users";

    public OnlineUserFilter(RedisClient redis)
    {
        _redis = redis;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. 先执行业务请求
        await next();

        // 2. 异步记录在线状态 (Fire-and-forget)，确保不影响主流程响应速度
        // 且包裹在 try-catch 中，确保 Redis 故障不影响用户
        _ = Task.Run(() =>
        {
            try
            {
                var user = context.HttpContext.User;
                if (user.Identity?.IsAuthenticated == true)
                {
                    var userId = user.FindFirst(ClaimTypes.Sid)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        // 使用 ZAdd 记录，Score 为当前秒级时间戳
                        var score = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        _redis.ZAdd(OnlineUsersKey, score, userId);
                    }
                }
            }
            catch
            {
                // 静默处理 Redis 异常，防止由于监控功能导致业务崩溃
            }
        });
    }
}
