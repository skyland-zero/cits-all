using MyApi.Domain.Shared.Exports;

namespace MyApi.Application.Exports.Dto;

public class ExportTaskChangedMessage
{
    public Guid TaskId { get; set; }

    public string ModuleKey { get; set; } = string.Empty;

    public ExportTaskStatus Status { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? ErrorMessage { get; set; }
}
