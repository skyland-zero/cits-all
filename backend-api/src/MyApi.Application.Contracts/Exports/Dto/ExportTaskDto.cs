using MyApi.Domain.Shared.Exports;

namespace MyApi.Application.Exports.Dto;

public class ExportTaskDto
{
    public Guid Id { get; set; }

    public string ModuleKey { get; set; } = string.Empty;

    public string ModuleName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public ExportTaskStatus Status { get; set; }

    public int TotalCount { get; set; }

    public long FileSize { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? StartedTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public string? ErrorMessage { get; set; }

    public bool CanDownload { get; set; }
}
