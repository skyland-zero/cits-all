using Cits.Entities;
using FreeSql.DataAnnotations;
using MyApi.Domain.Shared.Exports;

namespace MyApi.Domain.Exports;

public class ExportTask : EntityBase
{
    [Column(StringLength = 100)]
    public string ModuleKey { get; set; } = string.Empty;

    [Column(StringLength = 100)]
    public string ModuleName { get; set; } = string.Empty;

    [Column(StringLength = 200)]
    public string FileName { get; set; } = string.Empty;

    [Column(StringLength = -1)]
    public string QueryJson { get; set; } = "{}";

    [Column(StringLength = -1)]
    public string FieldsJson { get; set; } = "[]";

    [Column(MapType = typeof(int))]
    public ExportTaskStatus Status { get; set; } = ExportTaskStatus.Pending;

    public int TotalCount { get; set; }

    public DateTime? StartedTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    [Column(StringLength = 500)]
    public string? ErrorMessage { get; set; }

    [Column(StringLength = 300)]
    public string? RootIdentifier { get; set; }

    [Column(StringLength = 500)]
    public string? RelativePath { get; set; }

    [Column(StringLength = 200)]
    public string? StorageName { get; set; }

    [Column(StringLength = 100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }
}
