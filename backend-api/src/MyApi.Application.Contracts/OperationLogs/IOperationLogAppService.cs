using Cits.DI;
using MyApi.Application.OperationLogs.Dto;

namespace MyApi.Application.OperationLogs;

public interface IOperationLogAppService : IApplicationService
{
    Task<OperationLogCursorResultDto> GetListAsync(GetOperationLogsInput input);

    Task<OperationLogDetailDto?> GetAsync(Guid id);
}