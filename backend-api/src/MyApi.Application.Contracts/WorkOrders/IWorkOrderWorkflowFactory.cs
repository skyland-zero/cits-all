using Cits.DI;

namespace MyApi.Application.Contracts.WorkOrders;

public interface IWorkOrderWorkflowFactory : IApplicationService
{
    IWorkOrderWorkflowService Create(Guid orderId);
}