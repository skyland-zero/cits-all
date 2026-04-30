using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MyApi.HttpApi.Hubs;

[Authorize]
public class ExportTaskHub : Hub
{
    public const string TaskChangedEvent = "ExportTaskChanged";

    public override async Task OnConnectedAsync()
    {
        var userIdValue = Context.User?.FindFirst(ClaimTypes.Sid)?.Value;
        var moduleKey = Context.GetHttpContext()?.Request.Query["moduleKey"].ToString();

        if (!Guid.TryParse(userIdValue, out var userId) || string.IsNullOrWhiteSpace(moduleKey))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, BuildGroupName(userId, moduleKey));
        await base.OnConnectedAsync();
    }

    public static string BuildGroupName(Guid userId, string moduleKey)
    {
        return $"export-tasks:{userId:N}:{moduleKey.Trim().ToLowerInvariant()}";
    }
}
