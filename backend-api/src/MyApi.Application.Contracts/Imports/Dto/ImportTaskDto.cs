using Cits.Dtos;
using MyApi.Domain.Shared.Imports;

namespace MyApi.Application.Imports.Dto;

public class ImportModuleDto
{
    public string ModuleKey { get; set; } = string.Empty;

    public string ModuleName { get; set; } = string.Empty;

}

public class ImportTemplateColumnDto
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public bool Required { get; set; }

    public string? Description { get; set; }
}

public class ImportTaskDto : EntityDto<Guid>
{
    public string ModuleKey { get; set; } = string.Empty;

    public string ModuleName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public ImportTaskStatus Status { get; set; }

    public int TotalCount { get; set; }

    public int SuccessCount { get; set; }

    public int FailedCount { get; set; }

    public long FileSize { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? StartedTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public string? ErrorMessage { get; set; }

    public bool CanDownloadErrorReport { get; set; }
}

public class GetImportTasksInput : PagedRequestDto
{
    public string? ModuleKey { get; set; }
}

public class ImportRowErrorDto
{
    public int RowNumber { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Dictionary<string, string?> Values { get; set; } = [];
}

public class ImportProviderResult
{
    public int TotalCount { get; set; }

    public int SuccessCount { get; set; }

    public List<ImportRowErrorDto> Errors { get; set; } = [];
}
