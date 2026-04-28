using Cits.Dtos;

namespace MyApi.Application.OperationLogs.Dto;

public class OperationLogCursorResultDto : ListResultDto<OperationLogCursorItemDto>
{
    public bool HasMore { get; set; }

    public string? NextCursor { get; set; }

    public DateTime? NextCursorTime { get; set; }

    public Guid? NextCursorId { get; set; }
}