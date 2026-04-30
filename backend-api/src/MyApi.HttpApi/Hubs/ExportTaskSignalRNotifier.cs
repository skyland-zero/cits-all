using Microsoft.AspNetCore.SignalR;
using MyApi.Application.Exports;
using MyApi.Application.Exports.Dto;

namespace MyApi.HttpApi.Hubs;

public class ExportTaskSignalRNotifier : IExportTaskNotifier
{
    private readonly IHubContext<ExportTaskHub> _hubContext;
    private readonly ILogger<ExportTaskSignalRNotifier> _logger;

    public ExportTaskSignalRNotifier(
        IHubContext<ExportTaskHub> hubContext,
        ILogger<ExportTaskSignalRNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyTaskChangedAsync(
        Guid creatorUserId,
        string moduleKey,
        ExportTaskChangedMessage message,
        CancellationToken cancellationToken = default)
    {
        if (creatorUserId == Guid.Empty || string.IsNullOrWhiteSpace(moduleKey))
        {
            return;
        }

        try
        {
            await _hubContext.Clients
                .Group(ExportTaskHub.BuildGroupName(creatorUserId, moduleKey))
                .SendAsync(ExportTaskHub.TaskChangedEvent, message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "导出任务状态推送失败: {TaskId}", message.TaskId);
        }
    }
}
