using Cits.Entities;
using FreeSql.DataAnnotations;
using MyApi.Domain.Shared.Imports;

namespace MyApi.Domain.Imports;

public class ImportTask : EntityBase
{
    [Column(StringLength = 100)]
    public string ModuleKey { get; set; } = string.Empty;

    [Column(StringLength = 100)]
    public string ModuleName { get; set; } = string.Empty;

    [Column(StringLength = 300)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Column(StringLength = 300)]
    public string StorageName { get; set; } = string.Empty;

    [Column(StringLength = 300)]
    public string RootIdentifier { get; set; } = string.Empty;

    [Column(StringLength = 500)]
    public string RelativePath { get; set; } = string.Empty;

    [Column(StringLength = 100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [Column(MapType = typeof(int))]
    public ImportTaskStatus Status { get; set; } = ImportTaskStatus.Pending;

    public int TotalCount { get; set; }

    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public DateTime? StartedTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    [Column(StringLength = 500)]
    public string? ErrorMessage { get; set; }

    [Column(StringLength = 300)]
    public string? ErrorReportRootIdentifier { get; set; }

    [Column(StringLength = 500)]
    public string? ErrorReportRelativePath { get; set; }

    [Column(StringLength = 300)]
    public string? ErrorReportStorageName { get; set; }

    public long ErrorReportFileSize { get; set; }
}
