using System.ComponentModel.DataAnnotations;

namespace MyApi.Application.OperationLogs.Dto;

public class GetOperationLogsInput
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Module { get; set; }

    public string? OperationType { get; set; }

    public string? OperatorId { get; set; }

    public bool? Status { get; set; }

    public string? Cursor { get; set; }

    public DateTime? CursorTime { get; set; }

    public Guid? CursorId { get; set; }

    [Range(1, 200)]
    public int MaxResultCount { get; set; } = 50;
}