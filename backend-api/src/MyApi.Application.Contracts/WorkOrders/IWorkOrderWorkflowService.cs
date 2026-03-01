using MyApi.Domain.Shared.WorkOrders;

namespace MyApi.Application.Contracts.WorkOrders;

public interface IWorkOrderWorkflowService
{
    Task<bool> FireAsync(WorkOrderTrigger trigger, Guid operatorId, string remark = "");
    Task<bool> AssignAsync(Guid assigneeId, Guid operatorId, string remark = "");
}