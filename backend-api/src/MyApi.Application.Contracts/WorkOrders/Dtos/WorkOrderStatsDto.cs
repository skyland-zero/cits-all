using System.Collections.Generic;

namespace MyApi.Application.Contracts.WorkOrders.Dtos;

/// <summary>
/// 工单统计信息 DTO
/// </summary>
public class WorkOrderStatsDto
{
    /// <summary>
    /// 总工单数
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// 状态分布
    /// </summary>
    public List<StatItemDto> StatusDistribution { get; set; } = new();

    /// <summary>
    /// 优先级分布
    /// </summary>
    public List<StatItemDto> PriorityDistribution { get; set; } = new();

    /// <summary>
    /// 结单率 (0.00 - 1.00)
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// 平均处理时长（分钟）
    /// </summary>
    public double AverageProcessingTimeMinutes { get; set; }
}

public class StatItemDto
{
    public int Key { get; set; }
    public string Label { get; set; } = string.Empty;
    public long Count { get; set; }
}
