using Cits.Dtos;
using MyApi.Domain.Shared.WorkOrders;

namespace MyApi.Application.Contracts.WorkOrders.Dtos;

/// <summary>
/// 工单详情 DTO
/// </summary>
public class WorkOrderDto : EntityDto<Guid>
{
    /// <summary>
    /// 工单业务编号
    /// </summary>
    public string? OrderNo { get; set; }

    /// <summary>
    /// 工单标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public WorkOrderPriority Priority { get; set; }

    /// <summary>
    /// 工单描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 故障/作业地点
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 联系人姓名
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系人电话
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 附件（JSON 数组存储）
    /// </summary>
    public string? Attachments { get; set; }

    /// <summary>
    /// 附件列表对象（供前端展示）
    /// </summary>
    public List<FileAttachmentDto>? AttachmentList { get; set; }

    /// <summary>
    /// 当前状态
    /// </summary>
    public WorkOrderStatus CurrentStatus { get; set; }

    /// <summary>
    /// 发起人用户 ID
    /// </summary>
    public Guid RequesterId { get; set; }

    /// <summary>
    /// 当前处理人用户 ID
    /// </summary>
    public Guid ProcessorId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 处理截止时间
    /// </summary>
    public DateTime? DeadlineTime { get; set; }
}
