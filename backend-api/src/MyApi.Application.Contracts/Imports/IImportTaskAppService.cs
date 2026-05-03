using Cits.DI;
using Cits.Dtos;
using MyApi.Application.Imports.Dto;

namespace MyApi.Application.Imports;

public interface IImportTaskAppService : IApplicationService
{
    Task<ListResultDto<ImportModuleDto>> GetModulesAsync();

    Task<(byte[] Bytes, string FileName)> BuildTemplateAsync(string moduleKey);

    Task<ImportTaskDto> CreateAsync(string moduleKey, Stream stream, string fileName, string contentType, long fileSize);

    Task<PagedResultDto<ImportTaskDto>> GetListAsync(GetImportTasksInput input);
}
