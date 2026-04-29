using Cits.Entities;
using FreeSql.DataAnnotations;
using MyApi.Domain.Shared.WorkOrders;

namespace MyApi.Domain.WorkOrders;

/// <summary>
/// 工单主表实体
/// </summary>
public class WorkOrder : EntityBaseWithSoftDelete
{

    /// <summary>
    /// 工单业务编号（展示用）
    /// </summary>
    [Column(StringLength = 30, IsNullable = false)]
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 工单标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 优先级
    /// </summary>
    public WorkOrderPriority Priority { get; set; }

    /// <summary>
    /// 工单描述
    /// </summary>
    [Column(StringLength = -1)] // Text
    public string? Description { get; set; }

    /// <summary>
    /// 故障/作业地点
    /// </summary>
    [Column(StringLength = 200)]
    public string? Location { get; set; }

    /// <summary>
    /// 联系人姓名
    /// </summary>
    [Column(StringLength = 50)]
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系人电话
    /// </summary>
    [Column(StringLength = 20)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 附件（JSON 数组存储）
    /// </summary>
    [Column(StringLength = -1)]
    public string? Attachments { get; set; }

    /// <summary>
    /// 当前状态
    /// </summary>
    [Column(MapType = typeof(int))]
    public WorkOrderStatus CurrentStatus { get; set; }

    /// <summary>
    /// 发起人用户 ID
    /// </summary>
    [Column(StringLength = 50)]
    public Guid RequesterId { get; set; }

    /// <summary>
    /// 当前处理人用户 ID
    /// </summary>
    [Column(StringLength = 50)]
    public Guid ProcessorId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Column(ServerTime = DateTimeKind.Local, CanUpdate = false)]
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 处理截止时间（SLA）
    /// </summary>
    public DateTime? DeadlineTime { get; set; }
}
