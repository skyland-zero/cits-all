using System.ComponentModel;

namespace MyApi.Domain.Shared.WorkOrders;

/// <summary>
/// 工单优先级
/// </summary>
public enum WorkOrderPriority
{
    [Description("低")]
    Low = 0,

    [Description("中")]
    Normal = 1,

    [Description("高")]
    High = 2,

    [Description("紧急")]
    Urgent = 3
}
