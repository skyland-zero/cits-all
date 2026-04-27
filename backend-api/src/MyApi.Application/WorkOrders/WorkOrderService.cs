using Cits;
using Cits.Dtos;
using Cits.IdGenerator;
using Mapster;
using MyApi.Application.Contracts.WorkOrders;
using MyApi.Application.Contracts.WorkOrders.Dtos;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.FileUpload;
using MyApi.Domain.DomainServices.WorkOrders;
using MyApi.Domain.Shared.WorkOrders;
using MyApi.Domain.WorkOrders;
using System.Text.Json;

namespace MyApi.Application.WorkOrders;

/// <summary>
/// 工单管理服务
/// </summary>
public class WorkOrderService : IWorkOrderService
{
    private static readonly JsonSerializerOptions AttachmentJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IFreeSql _fsql;
    private readonly IIdGenerator _idGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly FileAccessSignatureService _fileAccessSignatureService;
    private readonly WorkOrderNoDomainService _workOrderNoDomainService;

    public WorkOrderService(IFreeSql fsql, IIdGenerator idGenerator, ICurrentUser currentUser,
        WorkOrderNoDomainService workOrderNoDomainService, FileAccessSignatureService fileAccessSignatureService)
    {
        _fsql = fsql;
        _idGenerator = idGenerator;
        _currentUser = currentUser;
        _workOrderNoDomainService = workOrderNoDomainService;
        _fileAccessSignatureService = fileAccessSignatureService;
    }

    /// <summary>
    /// 获取工单详情
    /// </summary>
    public async Task<WorkOrderDto> GetAsync(Guid id)
    {
        var entity = await _fsql.Select<WorkOrder>().Where(x => x.Id == id).FirstAsync();
        if (entity == null) throw new Exception("未找到指定工单");

        var dto = entity.Adapt<WorkOrderDto>();

        // 解析附件信息
        if (!string.IsNullOrEmpty(entity.Attachments))
        {
            try
            {
                var ids = ParseAttachmentIds(entity.Attachments);
                if (ids.Count > 0)
                {
                    var attachments = await _fsql.Select<FileAttachment>()
                        .Where(x => ids.Contains(x.Id))
                        .ToListAsync();

                    dto.AttachmentList = ids
                        .Select(id => attachments.FirstOrDefault(x => x.Id == id))
                        .Where(x => x != null)
                        .Select(x => x!)
                        .Select(ToAttachmentDto)
                        .ToList();
                }
            }
            catch
            {
                // 忽略解析错误，防止阻塞主流程
            }
        }

        return dto;
    }

    private class AttachmentInfo
    {
        public Guid Id { get; set; }
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
        await MarkAttachmentsAsPermanentAsync(input.Attachments);

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

        var removedAttachmentIds = GetRemovedAttachmentIds(entity.Attachments, input.Attachments);
        input.Adapt(entity);

        await _fsql.Update<WorkOrder>().SetSource(entity).ExecuteAffrowsAsync();
        await MarkAttachmentsAsPermanentAsync(input.Attachments);
        await RevokeAttachmentsAsync(removedAttachmentIds);
    }

    /// <summary>
    /// 获取工单统计数据
    /// </summary>
    public async Task<WorkOrderStatsDto> GetStatsAsync()
    {
        var stats = new WorkOrderStatsDto();

        // 1. 基础统计
        stats.TotalCount = await _fsql.Select<WorkOrder>().CountAsync();
        if (stats.TotalCount == 0) return stats;

        // 2. 状态分布
        var statusStats = await _fsql.Select<WorkOrder>()
            .GroupBy(x => x.CurrentStatus)
            .ToListAsync(x => new { Status = x.Key, Count = x.Count() });
        
        stats.StatusDistribution = statusStats.Select(x => new StatItemDto
        {
            Key = (int)x.Status,
            Label = x.Status.ToString(),
            Count = x.Count
        }).ToList();

        // 3. 优先级分布
        var priorityStats = await _fsql.Select<WorkOrder>()
            .GroupBy(x => x.Priority)
            .ToListAsync(x => new { Priority = x.Key, Count = x.Count() });

        stats.PriorityDistribution = priorityStats.Select(x => new StatItemDto
        {
            Key = (int)x.Priority,
            Label = x.Priority.ToString(),
            Count = x.Count
        }).ToList();

        // 4. 结单率
        var completedCount = statusStats.FirstOrDefault(x => x.Status == WorkOrderStatus.Completed)?.Count ?? 0;
        stats.CompletionRate = (double)completedCount / stats.TotalCount;

        // 5. 平均处理时长 (从创建到结单)
        // 简单实现：查询所有已完成工单的流转日志，计算创建时间到结单时间的差值
        var completionLogs = await _fsql.Select<WorkOrderLog>()
            .Where(x => x.ToStatus == WorkOrderStatus.Completed)
            .ToListAsync();

        if (completionLogs.Count > 0)
        {
            var orderIds = completionLogs.Select(x => x.WorkOrderId).ToList();
            var orders = await _fsql.Select<WorkOrder>().Where(x => orderIds.Contains(x.Id)).ToListAsync();
            
            double totalMinutes = 0;
            int validCount = 0;
            foreach (var log in completionLogs)
            {
                var order = orders.FirstOrDefault(o => o.Id == log.WorkOrderId);
                if (order != null)
                {
                    totalMinutes += (log.CreateTime - order.CreatedTime).TotalMinutes;
                    validCount++;
                }
            }
            if (validCount > 0)
            {
                stats.AverageProcessingTimeMinutes = totalMinutes / validCount;
            }
        }

        return stats;
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

        await RevokeAttachmentsAsync(ParseAttachmentIds(entity.Attachments));
        await _fsql.Delete<WorkOrder>().Where(x => x.Id == id).ExecuteAffrowsAsync();
    }

    private FileAttachmentDto ToAttachmentDto(FileAttachment attachment)
    {
        return new FileAttachmentDto
        {
            Id = attachment.Id,
            OriginalName = attachment.OriginalName,
            StorageName = attachment.StorageName,
            Extension = attachment.Extension,
            FileSize = attachment.FileSize,
            RelativePath = attachment.RelativePath,
            Url = _fileAccessSignatureService.CreatePreviewUrl(attachment.Id, attachment.AccessVersion),
            DownloadUrl = _fileAccessSignatureService.CreateDownloadUrl(attachment.Id, attachment.AccessVersion),
        };
    }

    private async Task MarkAttachmentsAsPermanentAsync(string? attachments)
    {
        var ids = ParseAttachmentIds(attachments);
        if (ids.Count == 0)
        {
            return;
        }

        var dbAttachments = await _fsql.Select<FileAttachment>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
        if (dbAttachments.Count != ids.Count)
        {
            throw new UserFriendlyException("存在无效的附件标识，请重新上传附件");
        }

        foreach (var attachment in dbAttachments)
        {
            attachment.IsPermanent = true;
        }

        await _fsql.Update<FileAttachment>().SetSource(dbAttachments).ExecuteAffrowsAsync();
    }

    private List<Guid> GetRemovedAttachmentIds(string? previousAttachments, string? currentAttachments)
    {
        var previousIds = ParseAttachmentIds(previousAttachments);
        if (previousIds.Count == 0)
        {
            return [];
        }

        var currentIds = ParseAttachmentIds(currentAttachments);
        return previousIds.Except(currentIds).ToList();
    }

    private async Task RevokeAttachmentsAsync(List<Guid> attachmentIds)
    {
        if (attachmentIds.Count == 0)
        {
            return;
        }

        var dbAttachments = await _fsql.Select<FileAttachment>()
            .Where(x => attachmentIds.Contains(x.Id))
            .ToListAsync();
        if (dbAttachments.Count == 0)
        {
            return;
        }

        foreach (var attachment in dbAttachments)
        {
            attachment.IsPermanent = false;
            attachment.AccessVersion += 1;
        }

        await _fsql.Update<FileAttachment>().SetSource(dbAttachments).ExecuteAffrowsAsync();
    }

    private static List<Guid> ParseAttachmentIds(string? attachments)
    {
        if (string.IsNullOrWhiteSpace(attachments))
        {
            return [];
        }

        try
        {
            var attachInfos = JsonSerializer.Deserialize<List<AttachmentInfo>>(attachments, AttachmentJsonOptions);
            return attachInfos?
                .Select(x => x.Id)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList() ?? [];
        }
        catch (JsonException ex)
        {
            throw new UserFriendlyException("附件数据格式不正确", ex);
        }
    }
}
