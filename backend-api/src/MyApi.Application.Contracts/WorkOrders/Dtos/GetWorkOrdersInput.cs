using System;
using Cits.Dtos;
using MyApi.Domain.Shared.WorkOrders;

namespace MyApi.Application.Contracts.WorkOrders.Dtos;

/// <summary>
/// 工单分页查询输入参数
/// </summary>
public class GetWorkOrdersInput : PagedRequestDto
{
    /// <summary>
    /// 关键词（搜索标题或单号）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 工单状态过滤
    /// </summary>
    public WorkOrderStatus? Status { get; set; }

    /// <summary>
    /// 优先级过滤
    /// </summary>
    public WorkOrderPriority? Priority { get; set; }

    /// <summary>
    /// 发起人 ID 过滤
    /// </summary>
    public Guid? RequesterId { get; set; }

    /// <summary>
    /// 处理人 ID 过滤
    /// </summary>
    public Guid? ProcessorId { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
