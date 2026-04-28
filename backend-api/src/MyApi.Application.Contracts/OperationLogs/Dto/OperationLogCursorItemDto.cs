using Cits.Dtos;

namespace MyApi.Application.OperationLogs.Dto;

public class OperationLogCursorItemDto : EntityDto<Guid>
{
    public string? Module { get; set; }

    public string? OperationType { get; set; }

    public string? OperatorId { get; set; }

    public string? OperatorName { get; set; }

    public string? DepartmentPath { get; set; }

    public string? OperationIp { get; set; }

    public string? OperationLocation { get; set; }

    public bool Status { get; set; }

    public DateTime OperationTime { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string? RequestPath { get; set; }

    public string? RequestMethod { get; set; }
}