using Cits.IdGenerator;
using MyApi.Application.Contracts.WorkOrders;

namespace MyApi.Application.WorkOrders;

public class WorkOrderWorkflowFactory : IWorkOrderWorkflowFactory
{
    private readonly IFreeSql _fsql;
    private readonly IIdGenerator _idGenerator;

    public WorkOrderWorkflowFactory(IFreeSql fsql, IIdGenerator idGenerator)
    {
        _fsql = fsql;
        _idGenerator = idGenerator;
    }

    public IWorkOrderWorkflowService Create(Guid orderId)
    {
        return new WorkOrderWorkflowService(_fsql, _idGenerator, orderId);
    }
}