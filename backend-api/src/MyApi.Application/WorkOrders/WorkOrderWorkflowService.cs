using Cits.IdGenerator;
using FreeSql;
using MyApi.Application.Contracts.WorkOrders;
using MyApi.Domain.DomainServices.CorpWx;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.DomainServices.WorkOrders;
using MyApi.Domain.Shared.WorkOrders;
using MyApi.Domain.WorkOrders;
using MyApi.Domain.Identities;
using MyApi.Domain.FileUpload;
using Stateless;

namespace MyApi.Application.WorkOrders;

/// <summary>
/// 工单工作流与持久化服务
/// </summary>
public class WorkOrderWorkflowService : IWorkOrderWorkflowService
{
    private static readonly System.Text.Json.JsonSerializerOptions AttachmentJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IFreeSql _fsql;
    private readonly IIdGenerator _idGenerator;
    private readonly ICorpWxClient _corpWxClient;
    private readonly PdfAntiCounterfeitService _pdfAntiCounterfeitService;
    private readonly WorkOrderPdfGenerator _workOrderPdfGenerator;
    private readonly IStorageProvider _storageProvider;
    private readonly Guid _orderId;
    private WorkOrder _order = null!;
    private StateMachine<WorkOrderStatus, WorkOrderTrigger> _machine = null!;

    /// <summary>
    /// 初始化工作流服务
    /// </summary>
    public WorkOrderWorkflowService(
        IFreeSql fsql, 
        IIdGenerator idGenerator, 
        ICorpWxClient corpWxClient,
        PdfAntiCounterfeitService pdfAntiCounterfeitService,
        WorkOrderPdfGenerator workOrderPdfGenerator,
        IStorageProvider storageProvider,
        Guid orderId)
    {
        _fsql = fsql;
        _idGenerator = idGenerator;
        _corpWxClient = corpWxClient;
        _pdfAntiCounterfeitService = pdfAntiCounterfeitService;
        _workOrderPdfGenerator = workOrderPdfGenerator;
        _storageProvider = storageProvider;
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
                var orderRepo = _fsql.GetRepository<WorkOrder>();
                orderRepo.UnitOfWork = uow;
                orderRepo.Update(_order);

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
                var logRepo = _fsql.GetRepository<WorkOrderLog>();
                logRepo.UnitOfWork = uow;
                logRepo.Insert(log);

                // 3. 特殊逻辑：审核通过时生成带防伪码的 PDF 报告
                if (trigger == WorkOrderTrigger.Approve)
                {
                    await ProcessSecureReportAsync(_order, uow);
                }

                uow.Commit();

                // 异步发送通知
                _ = NotifyAsync(trigger, operatorId, remark);

                return true;
            }
            catch
            {
                uow.Rollback();
                throw;
            }
        }
    }

    private async Task NotifyAsync(WorkOrderTrigger trigger, Guid operatorId, string remark)
    {
        try
        {
            switch (trigger)
            {
                case WorkOrderTrigger.Assign:
                    // 通知处理人
                    await SendCorpWxMsgAsync(_order.ProcessorId, $"您有新的工单任务：{_order.Title} (单号: {_order.OrderNo})");
                    break;
                case WorkOrderTrigger.Approve:
                    // 通知发起人
                    await SendCorpWxMsgAsync(_order.RequesterId, $"您的工单已审核通过并结单：{_order.Title} (单号: {_order.OrderNo})");
                    break;
                case WorkOrderTrigger.Reject:
                    // 通知处理人被驳回
                    await SendCorpWxMsgAsync(_order.ProcessorId, $"工单处理被驳回，请重新处理：{_order.Title} (单号: {_order.OrderNo})。备注：{remark}");
                    break;
            }
        }
        catch
        {
            // 通知失败不影响主业务
        }
    }

    private async Task SendCorpWxMsgAsync(Guid userId, string message)
    {
        if (userId == Guid.Empty) return;

        var user = await _fsql.Select<IdentityUser>().Where(x => x.Id == userId).FirstAsync();
        if (user != null && !string.IsNullOrEmpty(user.CorpWxUserId))
        {
            await _corpWxClient.SendAppMsg(user.CorpWxUserId, message);
        }
    }

    /// <summary>
    /// 处理 PDF 报告生成与防伪指纹注入
    /// </summary>
    private async Task ProcessSecureReportAsync(WorkOrder order, IUnitOfWork uow)
    {
        try
        {
            // 1. 生成防伪指纹
            var fingerprint = _pdfAntiCounterfeitService.GenerateFingerprint(
                order.OrderNo,
                order.ProcessorId,
                DateTime.Now);

            // 2. 生成 PDF 字节流
            var pdfBytes = await _workOrderPdfGenerator.GenerateAsync(order, fingerprint);
            if (pdfBytes == null || pdfBytes.Length == 0) return;

            // 3. 保存 PDF 文件
            var fileName = $"Report_{order.OrderNo}.pdf";
            var storageName = $"{Guid.NewGuid()}.pdf";
            
            using var ms = new MemoryStream(pdfBytes);
            var rootIdentifier = await _storageProvider.SaveAsync(ms, storageName, "application/pdf", null);
            
            var relativePath = Path.Combine("business", storageName).Replace("\\", "/");

            // 4. 插入附件表
            var attachment = new FileAttachment
            {
                Id = _idGenerator.Create(),
                OriginalName = fileName,
                FileHash = fingerprint, // 将指纹存入 Hash 字段便于检索核验
                StorageName = storageName,
                Extension = ".pdf",
                FileSize = pdfBytes.Length,
                ContentType = "application/pdf",
                RootIdentifier = rootIdentifier,
                RelativePath = relativePath,
                CreateTime = DateTime.Now,
                IsPermanent = true // 报告是永久有效的
            };
            var fileRepo = _fsql.GetRepository<FileAttachment>();
            fileRepo.UnitOfWork = uow;
            fileRepo.Insert(attachment);

            // 5. 更新工单附件信息（将报告加入附件列表）
            // 这里的逻辑可以根据业务调整，比如单独存放在 ReportFileId 字段
            var currentAttachments = new List<AttachmentItem>();
            if (!string.IsNullOrEmpty(order.Attachments))
            {
                try
                {
                    currentAttachments = System.Text.Json.JsonSerializer.Deserialize<List<AttachmentItem>>(order.Attachments, AttachmentJsonOptions) ?? new();
                }
                catch { }
            }
            
            currentAttachments.Add(new AttachmentItem { Id = attachment.Id, Name = "[作业报告]" + fileName });
            order.Attachments = System.Text.Json.JsonSerializer.Serialize(currentAttachments);
            
            var orderUpdateRepo = _fsql.GetRepository<WorkOrder>();
            orderUpdateRepo.UnitOfWork = uow;
            orderUpdateRepo.Update(order);
        }
        catch (Exception)
        {
            // 报告生成失败记录日志，但不回滚主业务事务（避免因报告生成失败导致无法结单）
            // 实际项目中建议加入重试队列
        }
    }

    private class AttachmentItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
