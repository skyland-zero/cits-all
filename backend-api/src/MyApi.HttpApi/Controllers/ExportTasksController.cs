using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Cits.OperationLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Exports;
using MyApi.Application.Exports.Dto;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.Exports;
using MyApi.Domain.Shared.Exports;

namespace MyApi.HttpApi.Controllers;

[Authorize]
[SkipOperationLog]
[Route("api/export-tasks")]
public class ExportTasksController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    private readonly IExportTaskAppService _exportTaskAppService;
    private readonly IFreeSql _freeSql;
    private readonly IStorageProvider _storageProvider;

    public ExportTasksController(
        IExportTaskAppService exportTaskAppService,
        IFreeSql freeSql,
        IStorageProvider storageProvider,
        ICurrentUser currentUser)
    {
        _exportTaskAppService = exportTaskAppService;
        _freeSql = freeSql;
        _storageProvider = storageProvider;
        _currentUser = currentUser;
    }

    [HttpGet("fields/{moduleKey}")]
    public Task<ListResultDto<ExportFieldDto>> GetFieldsAsync(string moduleKey)
    {
        return _exportTaskAppService.GetFieldsAsync(moduleKey);
    }

    [HttpPost]
    public Task<ExportTaskDto> CreateAsync(CreateExportTaskInput input)
    {
        return _exportTaskAppService.CreateAsync(input);
    }

    [HttpGet]
    public Task<PagedResultDto<ExportTaskDto>> GetListAsync([FromQuery] GetExportTasksInput input)
    {
        return _exportTaskAppService.GetListAsync(input);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadAsync(Guid id)
    {
        var task = await _freeSql.Select<ExportTask>().Where(x => x.Id == id).FirstAsync();
        if (task is null)
        {
            return NotFound("导出任务不存在");
        }

        if (_currentUser.Id != Guid.Empty && task.CreatorUserId != _currentUser.Id)
        {
            return Forbid();
        }

        if (task.Status != ExportTaskStatus.Succeeded ||
            task.RootIdentifier.IsNullOrWhiteSpace() ||
            task.RelativePath.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("导出文件尚未生成");
        }

        var stream = await _storageProvider.GetStreamAsync(task.RootIdentifier!, task.RelativePath!);
        if (stream is null)
        {
            return NotFound("导出文件不存在");
        }

        return File(stream, task.ContentType ?? "text/csv; charset=utf-8", task.FileName);
    }
}
