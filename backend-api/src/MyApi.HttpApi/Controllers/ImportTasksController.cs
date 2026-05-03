using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Cits.OperationLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Imports;
using MyApi.Application.Imports.Dto;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.Imports;
using MyApi.Domain.Shared.Imports;

namespace MyApi.HttpApi.Controllers;

[Authorize]
[SkipOperationLog]
[Route("api/import-tasks")]
public class ImportTasksController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    private readonly IFreeSql _freeSql;
    private readonly IImportTaskAppService _importTaskAppService;
    private readonly IStorageProvider _storageProvider;

    public ImportTasksController(
        IImportTaskAppService importTaskAppService,
        IFreeSql freeSql,
        IStorageProvider storageProvider,
        ICurrentUser currentUser)
    {
        _importTaskAppService = importTaskAppService;
        _freeSql = freeSql;
        _storageProvider = storageProvider;
        _currentUser = currentUser;
    }

    [HttpGet("modules")]
    public Task<ListResultDto<ImportModuleDto>> GetModulesAsync()
    {
        return _importTaskAppService.GetModulesAsync();
    }

    [HttpGet("template/{moduleKey}")]
    public async Task<IActionResult> DownloadTemplateAsync(string moduleKey)
    {
        var (bytes, fileName) = await _importTaskAppService.BuildTemplateAsync(moduleKey);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("{moduleKey}")]
    public async Task<ImportTaskDto> CreateAsync(string moduleKey, IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new UserFriendlyException("导入文件不能为空");
        }

        await using var stream = file.OpenReadStream();
        return await _importTaskAppService.CreateAsync(moduleKey, stream, file.FileName, file.ContentType, file.Length);
    }

    [HttpGet]
    public Task<PagedResultDto<ImportTaskDto>> GetListAsync([FromQuery] GetImportTasksInput input)
    {
        return _importTaskAppService.GetListAsync(input);
    }

    [HttpGet("{id:guid}/error-report")]
    public async Task<IActionResult> DownloadErrorReportAsync(Guid id)
    {
        var task = await _freeSql.Select<ImportTask>().Where(x => x.Id == id).FirstAsync();
        if (task is null)
        {
            return NotFound("导入任务不存在");
        }

        if (_currentUser.Id != Guid.Empty && task.CreatorUserId != _currentUser.Id)
        {
            return Forbid();
        }

        if ((task.Status != ImportTaskStatus.Failed && task.Status != ImportTaskStatus.PartiallySucceeded) ||
            task.ErrorReportRootIdentifier.IsNullOrWhiteSpace() ||
            task.ErrorReportRelativePath.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("错误报告尚未生成");
        }

        var stream = await _storageProvider.GetStreamAsync(task.ErrorReportRootIdentifier!, task.ErrorReportRelativePath!);
        if (stream is null)
        {
            return NotFound("错误报告不存在");
        }

        var fileName = $"{task.ModuleName}导入错误报告_{task.CreationTime:yyyyMMddHHmmss}.xlsx";
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
