using Cits.DI;
using Cits.Dtos;
using MyApi.Application.FileManagement.Dto;

namespace MyApi.Application.FileManagement;

public interface IFileManagementAppService : IApplicationService
{
    Task<PagedResultDto<FileManagementDto>> GetListAsync(QueryFileManagementDto input);

    Task<FileManagementDto> GetAsync(Guid id);

    Task DeleteAsync(Guid id);

    Task<FileCleanupResultDto> BatchDeleteAsync(BatchDeleteFileDto input);

    Task<FileCleanupResultDto> CleanupTemporaryAsync(CleanupTemporaryFilesDto input);

    Task<FileManagementDto> ReplaceAsync(Guid sourceFileId, Stream stream, string fileName, string contentType, long fileSize);

    Task<List<FileReplacementRecordDto>> GetReplacementRecordsAsync(Guid fileId);
}
