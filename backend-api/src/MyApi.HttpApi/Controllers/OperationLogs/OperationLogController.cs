using Cits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.OperationLogs;
using MyApi.Application.OperationLogs.Dto;

namespace MyApi.HttpApi.Controllers.OperationLogs;

[Authorize]
public class OperationLogController : BaseApiController
{
    private readonly IOperationLogAppService _operationLogAppService;

    public OperationLogController(IOperationLogAppService operationLogAppService)
    {
        _operationLogAppService = operationLogAppService;
    }

    [HttpGet]
    public Task<OperationLogCursorResultDto> GetListAsync([FromQuery] GetOperationLogsInput input)
    {
        return _operationLogAppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public Task<OperationLogDetailDto?> GetAsync(Guid id)
    {
        return _operationLogAppService.GetAsync(id);
    }
}