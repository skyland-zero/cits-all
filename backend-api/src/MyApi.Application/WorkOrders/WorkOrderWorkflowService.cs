using Cits.IdGenerator;
using FreeSql;
using MyApi.Application.Contracts.WorkOrders;
using MyApi.Domain.Shared.WorkOrders;
using MyApi.Domain.WorkOrders;
using Stateless;

namespace MyApi.Application.WorkOrders;

/// <summary>
/// 工单工作流与持久化服务
/// </summary>
public class WorkOrderWorkflowService : IWorkOrderWorkflowService
{
    private readonly IFreeSql _fsql;
    private readonly IIdGenerator _idGenerator;
    private readonly Guid _orderId;
    private WorkOrder _order;
    private StateMachine<WorkOrderStatus, WorkOrderTrigger> _machine;

    /// <summary>
    /// 初始化工作流服务
    /// </summary>
    /// <param name="fsql">FreeSql 实例</param>
    /// <param name="orderId">工单 ID</param>
    public WorkOrderWorkflowService(IFreeSql fsql, IIdGenerator idGenerator, Guid orderId)
    {
        _fsql = fsql;
        _idGenerator = idGenerator;
        _orderId = orderId;
    }

    /// <summary>
    /// 加载数据并配置状态机规则
    /// </summary>
    private async Task LoadAndConfigureAsync()
    {
        _order = await _fsql.Select<WorkOrder>().Where(a => a.Id == _orderId).FirstAsync();
        if (_order == null) throw new Exception("未找到指定工单");

        _machine = new StateMachine<WorkOrderStatus, WorkOrderTrigger>(
            () => _order.CurrentStatus,
            s => _order.CurrentStatus = s);

        /// <summary>
        /// 定义：草稿 -> 待分派
        /// </summary>
        _machine.Configure(WorkOrderStatus.Draft)
            .Permit(WorkOrderTrigger.Submit, WorkOrderStatus.PendingAssignment);

        /// <summary>
        /// 定义：待分派 -> 处理中
        /// </summary>
        _machine.Configure(WorkOrderStatus.PendingAssignment)
            .Permit(WorkOrderTrigger.Assign, WorkOrderStatus.InProgress)
            .Permit(WorkOrderTrigger.Cancel, WorkOrderStatus.Canceled);

        /// <summary>
        /// 定义：处理中 -> 待审核
        /// </summary>
        _machine.Configure(WorkOrderStatus.InProgress)
            .Permit(WorkOrderTrigger.Finish, WorkOrderStatus.PendingApproval);

        /// <summary>
        /// 定义：待审核流程（通过或驳回）
        /// </summary>
        _machine.Configure(WorkOrderStatus.PendingApproval)
            .Permit(WorkOrderTrigger.Approve, WorkOrderStatus.Completed)
            .Permit(WorkOrderTrigger.Reject, WorkOrderStatus.InProgress);
    }

    /// <summary>
    /// 分派工单
    /// </summary>
    public async Task<bool> AssignAsync(Guid assigneeId, Guid operatorId, string remark = "")
    {
        return await ExecuteAsync(WorkOrderTrigger.Assign, operatorId, remark, order =>
        {
            order.ProcessorId = assigneeId;
        });
    }

    /// <summary>
    /// 执行工单状态转换
    /// </summary>
    public async Task<bool> FireAsync(WorkOrderTrigger trigger, Guid operatorId, string remark = "")
    {
        return await ExecuteAsync(trigger, operatorId, remark);
    }

    private async Task<bool> ExecuteAsync(WorkOrderTrigger trigger, Guid operatorId, string remark, Action<WorkOrder>? preAction = null)
    {
        await LoadAndConfigureAsync();

        if (!_machine.CanFire(trigger)) return false;

        var fromStatus = _order.CurrentStatus;

        using (var uow = _fsql.CreateUnitOfWork())
        {
            try
            {
                if (preAction != null) preAction(_order);

                await _machine.FireAsync(trigger);
                var toStatus = _order.CurrentStatus;

                // 1. 更新工单主表状态
                uow.GetRepository<WorkOrder>().Update(_order);

                // 2. 插入流转日志
                var log = new WorkOrderLog
                {
                    Id = _idGenerator.Create(),
                    WorkOrderId = _order.Id,
                    FromStatus = fromStatus,
                    ToStatus = toStatus,
                    ActionName = trigger.ToString(),
                    Remark = remark,
                    OperatorId = operatorId
                };
                uow.GetRepository<WorkOrderLog>().Insert(log);

                // 3. 特殊逻辑：审核通过时生成带防伪码的 PDF 报告
                if (trigger == WorkOrderTrigger.Approve)
                {
                    await ProcessSecureReportAsync(_order, uow);
                }

                uow.Commit();
                return true;
            }
            catch
            {
                uow.Rollback();
                throw;
            }
        }
    }

    /// <summary>
    /// 处理 PDF 报告生成与防伪指纹注入
    /// </summary>
    private async Task ProcessSecureReportAsync(WorkOrder order, IUnitOfWork uow)
    {
        /// <summary>
        /// 伪代码流程：
        /// 1. 获取报表字节数组：byte[] pdfBody = PdfGenerator.Create(order);
        /// 2. 注入防伪指纹：byte[] securedPdf = PdfAntiCounterfeitService.ApplyProtection(pdfBody);
        /// 3. 保存文件并记录 Hash 到数据库附件表
        /// </summary>
        await Task.CompletedTask;
    }
}