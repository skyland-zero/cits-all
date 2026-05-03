using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.FileManagement;
using MyApi.Application.FileManagement.Dto;

namespace MyApi.HttpApi.Controllers.FileManagement;

/// <summary>
/// 文件管理中心。
/// </summary>
[Route("api/system/files")]
[Authorize]
public class FileManagementController : BaseApiController
{
    private readonly IFileManagementAppService _fileManagementAppService;

    public FileManagementController(IFileManagementAppService fileManagementAppService)
    {
        _fileManagementAppService = fileManagementAppService;
    }

    [HttpGet]
    public Task<PagedResultDto<FileManagementDto>> GetListAsync([FromQuery] QueryFileManagementDto input)
    {
        return _fileManagementAppService.GetListAsync(input);
    }

    [HttpGet("{id:guid}")]
    public Task<FileManagementDto> GetAsync(Guid id)
    {
        return _fileManagementAppService.GetAsync(id);
    }

    [HttpDelete("{id:guid}")]
    public Task DeleteAsync(Guid id)
    {
        return _fileManagementAppService.DeleteAsync(id);
    }

    [HttpPost("{id:guid}/replace")]
    public async Task<FileManagementDto> ReplaceAsync(Guid id, IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new UserFriendlyException("替换文件不能为空");
        }

        await using var stream = file.OpenReadStream();
        return await _fileManagementAppService.ReplaceAsync(id, stream, file.FileName, file.ContentType, file.Length);
    }

    [HttpGet("{id:guid}/replacement-records")]
    public Task<List<FileReplacementRecordDto>> GetReplacementRecordsAsync(Guid id)
    {
        return _fileManagementAppService.GetReplacementRecordsAsync(id);
    }

    [HttpPost("batch-delete")]
    public Task<FileCleanupResultDto> BatchDeleteAsync([FromBody] BatchDeleteFileDto input)
    {
        return _fileManagementAppService.BatchDeleteAsync(input);
    }

    [HttpPost("cleanup-temporary")]
    public Task<FileCleanupResultDto> CleanupTemporaryAsync([FromBody] CleanupTemporaryFilesDto input)
    {
        return _fileManagementAppService.CleanupTemporaryAsync(input);
    }
}
