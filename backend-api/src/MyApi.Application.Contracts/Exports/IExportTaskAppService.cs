using Cits.Dtos;
using Cits.DI;
using MyApi.Application.Exports.Dto;

namespace MyApi.Application.Exports;

public interface IExportTaskAppService : IApplicationService
{
    Task<ListResultDto<ExportFieldDto>> GetFieldsAsync(string moduleKey);

    Task<ExportTaskDto> CreateAsync(CreateExportTaskInput input);

    Task<PagedResultDto<ExportTaskDto>> GetListAsync(GetExportTasksInput input);
}
