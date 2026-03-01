using System;
using System.ComponentModel.DataAnnotations;
using MyApi.Domain.Shared.WorkOrders;

namespace MyApi.Application.Contracts.WorkOrders.Dtos;

/// <summary>
/// 创建工单 DTO
/// </summary>
public class WorkOrderCreateDto
{
    /// <summary>
    /// 工单标题
    /// </summary>
    [Required(ErrorMessage = "工单标题不能为空")]
    [MaxLength(200, ErrorMessage = "工单标题不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

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
    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>
    /// 联系人姓名
    /// </summary>
    [MaxLength(50)]
    public string? ContactName { get; set; }

    /// <summary>
    /// 联系人电话
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// 附件（JSON 数组存储）
    /// </summary>
    public string? Attachments { get; set; }

    /// <summary>
    /// 处理截止时间
    /// </summary>
    public DateTime? DeadlineTime { get; set; }
}
