namespace MyApi.Domain.Shared.WorkOrders;

/// <summary>
/// 工单状态
/// </summary>
public enum WorkOrderStatus
{
    Draft = 0,               // 草稿
    PendingAssignment = 1,   // 待分派
    InProgress = 2,          // 处理中
    PendingApproval = 3,     // 待审核
    Completed = 4,           // 已完成
    Canceled = 5             // 已作废
}
