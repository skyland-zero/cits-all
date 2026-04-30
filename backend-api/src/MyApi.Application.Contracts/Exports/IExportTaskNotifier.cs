using MyApi.Application.Exports.Dto;

namespace MyApi.Application.Exports;

public interface IExportTaskNotifier
{
    Task NotifyTaskChangedAsync(
        Guid creatorUserId,
        string moduleKey,
        ExportTaskChangedMessage message,
        CancellationToken cancellationToken = default);
}
