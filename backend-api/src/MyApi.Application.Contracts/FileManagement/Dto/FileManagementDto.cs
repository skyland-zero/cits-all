using Cits.Dtos;

namespace MyApi.Application.FileManagement.Dto;

public class FileManagementDto : EntityDto<Guid>
{
    public string OriginalName { get; set; } = string.Empty;

    public string FileHash { get; set; } = string.Empty;

    public string StorageName { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public string RelativePath { get; set; } = string.Empty;

    public string RootIdentifier { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }

    public bool IsPermanent { get; set; }

    public int AccessVersion { get; set; }

    public string CreatorUserName { get; set; } = string.Empty;

    public string? PreviewUrl { get; set; }

    public string? DownloadUrl { get; set; }
}

public class QueryFileManagementDto : PagedRequestDto
{
    public string? Keyword { get; set; }

    public string? Extension { get; set; }

    public string? ContentType { get; set; }

    public bool? IsPermanent { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
}

public class BatchDeleteFileDto
{
    public List<Guid> Ids { get; set; } = [];
}

public class CleanupTemporaryFilesDto
{
    public int OlderThanHours { get; set; } = 24;
}

public class FileCleanupResultDto
{
    public int DeletedCount { get; set; }

    public int FailedCount { get; set; }

    public List<string> FailedMessages { get; set; } = [];
}

public class FileReplacementRecordDto : EntityDto<Guid>
{
    public Guid SourceFileId { get; set; }

    public Guid ReplacementFileId { get; set; }

    public string SourceOriginalName { get; set; } = string.Empty;

    public string SourceRelativePath { get; set; } = string.Empty;

    public long SourceFileSize { get; set; }

    public string SourceExtension { get; set; } = string.Empty;

    public string? SourcePreviewUrl { get; set; }

    public string? SourceDownloadUrl { get; set; }

    public string ReplacementOriginalName { get; set; } = string.Empty;

    public string ReplacementRelativePath { get; set; } = string.Empty;

    public long ReplacementFileSize { get; set; }

    public string ReplacementExtension { get; set; } = string.Empty;

    public string? ReplacementPreviewUrl { get; set; }

    public string? ReplacementDownloadUrl { get; set; }

    public DateTime ReplacedTime { get; set; }

    public Guid ReplacedByUserId { get; set; }

    public string ReplacedByUserName { get; set; } = string.Empty;
}
