namespace MyApi.Domain.Shared.WorkOrders;

public enum WorkOrderTrigger
{
    Submit,     // 提交
    Assign,     // 分派
    Start,      // 开始执行
    Finish,     // 完成上报
    Approve,    // 审核通过
    Reject,     // 驳回
    Cancel      // 作废
}