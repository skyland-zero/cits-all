using Cits.IdGenerator;
using MyApi.Application.Contracts.WorkOrders;
using MyApi.Domain.DomainServices.CorpWx;
using MyApi.Domain.DomainServices.WorkOrders;
using MyApi.Domain.DomainServices.FileUpload;

namespace MyApi.Application.WorkOrders;

public class WorkOrderWorkflowFactory : IWorkOrderWorkflowFactory
{
    private readonly IFreeSql _fsql;
    private readonly IIdGenerator _idGenerator;
    private readonly ICorpWxClient _corpWxClient;
    private readonly PdfAntiCounterfeitService _pdfAntiCounterfeitService;
    private readonly WorkOrderPdfGenerator _workOrderPdfGenerator;
    private readonly IStorageProvider _storageProvider;

    public WorkOrderWorkflowFactory(
        IFreeSql fsql, 
        IIdGenerator idGenerator, 
        ICorpWxClient corpWxClient,
        PdfAntiCounterfeitService pdfAntiCounterfeitService,
        WorkOrderPdfGenerator workOrderPdfGenerator,
        IStorageProvider storageProvider)
    {
        _fsql = fsql;
        _idGenerator = idGenerator;
        _corpWxClient = corpWxClient;
        _pdfAntiCounterfeitService = pdfAntiCounterfeitService;
        _workOrderPdfGenerator = workOrderPdfGenerator;
        _storageProvider = storageProvider;
    }

    public IWorkOrderWorkflowService Create(Guid orderId)
    {
        return new WorkOrderWorkflowService(
            _fsql, 
            _idGenerator, 
            _corpWxClient, 
            _pdfAntiCounterfeitService,
            _workOrderPdfGenerator,
            _storageProvider,
            orderId);
    }
}