using Cits;
using Cits.Dtos;
using Cits.IdGenerator;
using Mapster;
using MyApi.Application.Contracts.WorkOrders;
using MyApi.Application.Contracts.WorkOrders.Dtos;
using MyApi.Domain.DomainServices.WorkOrders;
using MyApi.Domain.Shared.WorkOrders;
using MyApi.Domain.WorkOrders;

namespace MyApi.Application.WorkOrders;

/// <summary>
/// 工单管理服务
/// </summary>
public class WorkOrderService : IWorkOrderService
{
    private readonly IFreeSql _fsql;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly WorkOrderNoDomainService _workOrderNoDomainService;

    public WorkOrderService(IFreeSql fsql, IIdGenerator idGenerator, ICurrentUser currentUser, WorkOrderNoDomainService workOrderNoDomainService)
    {
        _fsql = fsql;
        _idGenerator = idGenerator;
        _currentUser = currentUser;
        _workOrderNoDomainService = workOrderNoDomainService;
    }

    /// <summary>
    /// 获取工单详情
    /// </summary>
    public async Task<WorkOrderDto> GetAsync(Guid id)
    {
        var entity = await _fsql.Select<WorkOrder>().Where(x => x.Id == id).FirstAsync();
        if (entity == null) throw new Exception("未找到指定工单");

        return entity.Adapt<WorkOrderDto>();
    }

    /// <summary>
    /// 分页查询工单
    /// </summary>
    public async Task<PagedResultDto<WorkOrderDto>> GetListAsync(GetWorkOrdersInput input)
    {
        var query = _fsql.Select<WorkOrder>()
            .WhereIf(!string.IsNullOrEmpty(input.Keyword), x => x.Title.Contains(input.Keyword!) || x.OrderNo.Contains(input.Keyword!))
            .WhereIf(input.Status.HasValue, x => x.CurrentStatus == input.Status!.Value)
            .WhereIf(input.Priority.HasValue, x => x.Priority == input.Priority!.Value)
            .WhereIf(input.RequesterId.HasValue, x => x.RequesterId == input.RequesterId!.Value)
            .WhereIf(input.ProcessorId.HasValue, x => x.ProcessorId == input.ProcessorId!.Value)
            .WhereIf(input.StartTime.HasValue, x => x.CreatedTime >= input.StartTime!.Value)
            .WhereIf(input.EndTime.HasValue, x => x.CreatedTime <= input.EndTime!.Value);

        var total = await query.CountAsync();

        query = query.OrderByDescending(x => x.CreatedTime).PageBy(input);

        var items = await query.ToListAsync();

        return new PagedResultDto<WorkOrderDto>(total, items.Adapt<List<WorkOrderDto>>());
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    public async Task<Guid> CreateAsync(WorkOrderCreateDto input)
    {
        var entity = input.Adapt<WorkOrder>();
        entity.Id = _idGenerator.Create();
        entity.CreatedTime = DateTime.Now;
        // 生成业务单号
        entity.OrderNo = await _workOrderNoDomainService.GenerateAsync();
        entity.CurrentStatus = WorkOrderStatus.Draft;
        entity.RequesterId = _currentUser.Id;
        entity.ProcessorId = Guid.Empty; // 初始无处理人

        await _fsql.Insert(entity).ExecuteAffrowsAsync();

        return entity.Id;
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    public async Task UpdateAsync(Guid id, WorkOrderUpdateDto input)
    {
        var entity = await _fsql.Select<WorkOrder>().Where(x => x.Id == id).FirstAsync();
        if (entity == null) throw new Exception("未找到指定工单");

        // 只有草稿状态允许编辑
        if (entity.CurrentStatus != WorkOrderStatus.Draft)
        {
            throw new Exception("只有草稿状态的工单可以修改");
        }

        input.Adapt(entity);

        await _fsql.Update<WorkOrder>().SetSource(entity).ExecuteAffrowsAsync();
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _fsql.Select<WorkOrder>().Where(x => x.Id == id).FirstAsync();
        if (entity == null) return;

        // 只有草稿或作废状态允许删除
        if (entity.CurrentStatus != WorkOrderStatus.Draft && entity.CurrentStatus != WorkOrderStatus.Canceled)
        {
            throw new Exception("只有草稿或作废状态的工单可以删除");
        }

        await _fsql.Delete<WorkOrder>().Where(x => x.Id == id).ExecuteAffrowsAsync();
    }
}
