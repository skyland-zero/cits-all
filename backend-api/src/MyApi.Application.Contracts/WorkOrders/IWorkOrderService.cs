using Cits.DI;
using Cits.Dtos;
using MyApi.Application.Contracts.WorkOrders.Dtos;

namespace MyApi.Application.Contracts.WorkOrders;

/// <summary>
/// 工单管理服务接口
/// </summary>
public interface IWorkOrderService : IApplicationService
{
    /// <summary>
    /// 获取工单详情
    /// </summary>
    Task<WorkOrderDto> GetAsync(Guid id);

    /// <summary>
    /// 分页查询工单
    /// </summary>
    Task<PagedResultDto<WorkOrderDto>> GetListAsync(GetWorkOrdersInput input);

    /// <summary>
    /// 创建工单
    /// </summary>
    Task<Guid> CreateAsync(WorkOrderCreateDto input);

    /// <summary>
    /// 更新工单
    /// </summary>
    Task UpdateAsync(Guid id, WorkOrderUpdateDto input);

    /// <summary>
    /// 删除工单
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 获取工单统计数据
    /// </summary>
    Task<WorkOrderStatsDto> GetStatsAsync();
}