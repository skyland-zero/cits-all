using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Contracts.WorkOrders;
using MyApi.Application.Contracts.WorkOrders.Dtos;
using MyApi.Domain.Shared.WorkOrders;

namespace MyApi.HttpApi.Controllers.WorkOrders;

/// <summary>
/// 工单管理控制器
/// </summary>
public class WorkOrderController : WorkOrderBaseApiController
{

    private readonly ICurrentUser _currentUser;
    private readonly IWorkOrderService _workOrderService;

    public WorkOrderController(ICurrentUser currentUser, IWorkOrderService workOrderService)
    {
        _currentUser = currentUser;
        _workOrderService = workOrderService;
    }

    /// <summary>
    /// 获取工单详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<WorkOrderDto> GetAsync(Guid id)
    {
        return await _workOrderService.GetAsync(id);
    }

    /// <summary>
    /// 分页查询工单
    /// </summary>
    [HttpGet]
    public async Task<PagedResultDto<WorkOrderDto>> GetListAsync([FromQuery] GetWorkOrdersInput input)
    {
        return await _workOrderService.GetListAsync(input);
    }

    /// <summary>
    /// 创建工单
    /// </summary>
    [HttpPost]
    public async Task<Guid> CreateAsync([FromBody] WorkOrderCreateDto input)
    {
        return await _workOrderService.CreateAsync(input);
    }

    /// <summary>
    /// 更新工单
    /// </summary>
    [HttpPut("{id}")]
    public async Task UpdateAsync(Guid id, [FromBody] WorkOrderUpdateDto input)
    {
        await _workOrderService.UpdateAsync(id, input);
    }

    /// <summary>
    /// 删除工单
    /// </summary>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _workOrderService.DeleteAsync(id);
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(Guid id, [FromServices] IWorkOrderWorkflowFactory factory)
    {
        var workflow = factory.Create(id);
        var success = await workflow.FireAsync(WorkOrderTrigger.Submit, _currentUser.Id, "用户提交");

        return success ? Ok() : BadRequest("当前状态无法提交");
    }

    [HttpPost("{id}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] Guid assigneeId, [FromServices] IWorkOrderWorkflowFactory factory)
    {
        var workflow = factory.Create(id);
        var success = await workflow.AssignAsync(assigneeId, _currentUser.Id, "分派任务");

        return success ? Ok() : BadRequest("当前状态无法分派");
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(Guid id, [FromServices] IWorkOrderWorkflowFactory factory)
    {
        var workflow = factory.Create(id);
        var success = await workflow.FireAsync(WorkOrderTrigger.Start, _currentUser.Id, "开始处理");

        return success ? Ok() : BadRequest("当前状态无法开始");
    }

    [HttpPost("{id}/finish")]
    public async Task<IActionResult> Finish(Guid id, [FromServices] IWorkOrderWorkflowFactory factory)
    {
        var workflow = factory.Create(id);
        var success = await workflow.FireAsync(WorkOrderTrigger.Finish, _currentUser.Id, "处理完成");

        return success ? Ok() : BadRequest("当前状态无法完成");
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromServices] IWorkOrderWorkflowFactory factory)
    {
        var workflow = factory.Create(id);
        var success = await workflow.FireAsync(WorkOrderTrigger.Reject, _currentUser.Id, "审核驳回");

        return success ? Ok() : BadRequest("当前状态无法驳回");
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromServices] IWorkOrderWorkflowFactory factory)
    {
        var workflow = factory.Create(id);
        var success = await workflow.FireAsync(WorkOrderTrigger.Cancel, _currentUser.Id, "取消订单");

        return success ? Ok() : BadRequest("当前状态无法取消");
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromServices] IWorkOrderWorkflowFactory factory)
    {
        var workflow = factory.Create(id);
        var success = await workflow.FireAsync(WorkOrderTrigger.Approve, _currentUser.Id, "准予结单");

        return success ? Ok() : BadRequest("当前工单状态无法审核通过");
    }
}