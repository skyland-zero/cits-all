using Cits.Entities;
using FreeSql.DataAnnotations;
using MyApi.Domain.Shared.WorkOrders;

namespace MyApi.Domain.WorkOrders;

/// <summary>
/// 工单操作流转日志
/// </summary>
public class WorkOrderLog : EntityBaseWithSoftDelete
{

    /// <summary>
    /// 关联的工单 ID
    /// </summary>
    [Column(StringLength = 50)]
    public Guid WorkOrderId { get; set; }

    /// <summary>
    /// 变更前状态
    /// </summary>
    public WorkOrderStatus FromStatus { get; set; }

    /// <summary>
    /// 变更后状态
    /// </summary>
    public WorkOrderStatus ToStatus { get; set; }

    /// <summary>
    /// 执行的操作动作
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// 操作备注/审批意见
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 操作人 ID
    /// </summary>
    [Column(StringLength = 50)]
    public Guid OperatorId { get; set; }

    /// <summary>
    /// 日志创建时间
    /// </summary>
    [Column(ServerTime = DateTimeKind.Local)]
    public DateTime CreateTime { get; set; }
}