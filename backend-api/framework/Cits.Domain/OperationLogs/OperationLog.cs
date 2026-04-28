using System.ComponentModel.DataAnnotations;
using Cits.Entities;
using FreeSql.DataAnnotations;

namespace Cits.OperationLogs;

[Index("idx_operation_log_operation_time_id", "OperationTime,Id")]
[Index("idx_operation_log_operator_time_id", "OperatorId,OperationTime,Id")]
[Index("idx_operation_log_module_time_id", "Module,OperationTime,Id")]
[Index("idx_operation_log_status_time_id", "Status,OperationTime,Id")]
public class OperationLog : Entity
{
    [MaxLength(100)]
    public string? Module { get; set; }

    [MaxLength(100)]
    public string? OperationType { get; set; }

    [MaxLength(50)]
    public string? OperatorId { get; set; }

    [MaxLength(100)]
    public string? OperatorName { get; set; }

    [MaxLength(2000)]
    public string? DepartmentPath { get; set; }

    [MaxLength(64)]
    public string? OperationIp { get; set; }

    [MaxLength(500)]
    public string? OperationLocation { get; set; }

    public bool Status { get; set; }

    public DateTime OperationTime { get; set; }

    public long ElapsedMilliseconds { get; set; }

    [MaxLength(500)]
    public string? RequestPath { get; set; }

    [MaxLength(20)]
    public string? RequestMethod { get; set; }

    public string? RequestParameters { get; set; }

    public string? ResponseParameters { get; set; }

    public string? ErrorMessage { get; set; }
}