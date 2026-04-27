using Cits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyApi.Application.Contracts.WorkOrders.Dtos;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.DomainServices.FileUpload.Dto;
using MyApi.Domain.FileUpload;
using MyApi.HttpApi.Hubs;

namespace MyApi.HttpApi.Controllers.FileUpload;

[Route("api/basic/upload/[controller]")]
public class UploadController : BaseApiController
{
    private readonly ICurrentUser _currentUser;
    private readonly IStorageProvider _storage;
    private readonly IFreeSql _fsql;
    private readonly FileAccessSignatureService _fileAccessSignatureService;
    private readonly ILogger<UploadController> _logger;
    private readonly IOptionsMonitor<UploadOptions> _uploadOptions;
    private readonly FileValidationService _validator;
    private readonly IHubContext<UploadHub> _hub;

    public UploadController(ICurrentUser currentUser, IStorageProvider storage, IFreeSql fsql,
        FileValidationService validator, IHubContext<UploadHub> hub,
        FileAccessSignatureService fileAccessSignatureService, IOptionsMonitor<UploadOptions> uploadOptions,
        ILogger<UploadController> logger)
    {
        _currentUser = currentUser;
        _storage = storage;
        _fsql = fsql;
        _validator = validator;
        _hub = hub;
        _fileAccessSignatureService = fileAccessSignatureService;
        _logger = logger;
        _uploadOptions = uploadOptions;
    }

    [HttpGet("settings")]
    public IActionResult GetSettings()
    {
        return Ok(new
        {
            maxConcurrentUploads = _uploadOptions.CurrentValue.Client.MaxConcurrentUploads,
            signalREnabled = _uploadOptions.CurrentValue.SignalR.Enabled,
            hubUrl = "/hub/upload"
        });
    }

    /// <summary>
    /// 普通上传接口（支持 SignalR 进度）
    /// </summary>
    /// <param name="file">文件对象</param>
    /// <param name="connectionId">SignalR 连接 ID (从 Header 获取)</param>
    /// <param name="uploadUid">前端文件唯一标识</param>
    [HttpPost("single")]
    public async Task<FileAttachmentDto> UploadSingle(IFormFile file,
        [FromHeader(Name = "X-Connection-Id")] string? connectionId,
        [FromHeader(Name = "X-Upload-Uid")] string? uploadUid)
    {
        // 1. 安全校验
        var (isValid, msg) = await _validator.ValidateAsync(file);
        if (!isValid)
        {
            throw new UserFriendlyException(msg);
        }

        // 2. 准备文件元数据
        var ext = Path.GetExtension(file.FileName).ToLower();
        var storageName = $"{Guid.NewGuid()}{ext}"; // 重命名为 GUID 防止冲突

        // 3. 执行存储并实时推送进度
        string rootIdentifier;
        try
        {
            await using var stream = file.OpenReadStream();

            // 调用 Provider，传入回调 Action<int>
            rootIdentifier = await _storage.SaveAsync(stream, storageName, file.ContentType, (percent) =>
            {
                // 如果前端传了 ConnectionId，则通过 SignalR 推送进度
                if (_uploadOptions.CurrentValue.SignalR.Enabled && !string.IsNullOrEmpty(connectionId) && !string.IsNullOrEmpty(uploadUid))
                {
                    // Fire-and-forget: 不要 await，避免阻塞文件写入流
                    _ = _hub.Clients.Client(connectionId).SendAsync("UploadProgress", new UploadProgressMessage(uploadUid, percent));
                }
            }, HttpContext.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            throw new UserFriendlyException("上传已取消");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传文件存储失败: {FileName}", file.FileName);
            throw new UserFriendlyException("存储失败");
        }

        // 4. 计算相对路径 (需与 StorageProvider 内部逻辑保持一致)
        // 假设 StorageProvider 使用 "business" 作为业务目录
        var relativePath = Path.Combine("business", storageName).Replace("\\", "/");

        // 5. 保存到数据库
        var attachment = new FileAttachment
        {
            Id = Guid.NewGuid(),
            OriginalName = file.FileName,
            FileHash = "", // 普通上传可选计算 Hash，若不用于秒传可留空
            StorageName = storageName,
            Extension = ext,
            FileSize = file.Length,
            ContentType = file.ContentType,
            RootIdentifier = rootIdentifier,
            RelativePath = relativePath,
            CreateTime = DateTime.Now,
            IsPermanent = false // 默认为临时，待业务表单提交后更新为 true
        };

        await _fsql.Insert(attachment).ExecuteAffrowsAsync();

        return CreateDto(attachment);
    }

    [AllowAnonymous]
    [HttpGet("access/preview")]
    public async Task<IActionResult> Preview([FromQuery] string token)
    {
        var file = await ResolveFileAsync(token, isPreview: true);
        if (file is null)
        {
            return NotFound("附件不存在或访问链接无效");
        }

        var stream = await _storage.GetStreamAsync(file.RootIdentifier, file.RelativePath);
        if (stream is null) return NotFound("附件文件不存在");

        Response.Headers.ContentDisposition = $"inline; filename*=UTF-8''{Uri.EscapeDataString(file.OriginalName)}";
        return File(stream, GetContentType(file));
    }

    [AllowAnonymous]
    [HttpGet("access/download")]
    public async Task<IActionResult> Download([FromQuery] string token)
    {
        var file = await ResolveFileAsync(token, isPreview: false);
        if (file is null)
        {
            return NotFound("附件不存在或访问链接无效");
        }

        var stream = await _storage.GetStreamAsync(file.RootIdentifier, file.RelativePath);
        if (stream is null) return NotFound("附件文件不存在");

        return File(stream, GetContentType(file), file.OriginalName);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTemporary(Guid id)
    {
        var file = await _fsql.Select<FileAttachment>().Where(x => x.Id == id).FirstAsync();
        if (file == null)
        {
            return NoContent();
        }

        if (file.IsPermanent)
        {
            throw new UserFriendlyException("正式附件不允许通过临时删除接口移除");
        }

        if (_currentUser.Id != Guid.Empty && file.CreatorUserId != Guid.Empty && file.CreatorUserId != _currentUser.Id)
        {
            throw new UserFriendlyException("无权删除该附件");
        }

        await _storage.DeleteAsync(file.RootIdentifier, file.RelativePath);
        await _fsql.Delete<FileAttachment>().Where(x => x.Id == id).ExecuteAffrowsAsync();
        return NoContent();
    }

    // 1. 检查秒传/断点
    [HttpGet("check/{hash}")]
    public async Task<IActionResult> Check(string hash)
    {
        // 检查秒传
        var file = await _fsql.Select<FileAttachment>().Where(a => a.FileHash == hash && a.IsPermanent).ToOneAsync();
        if (file != null) return Ok(new { complete = true, file });

        // 检查断点 (简单起见，返回已上传的分片索引)
        // 真实项目中这里应该去 Storage 检查文件夹下的文件列表
        // 为了演示，这里假设前端自己知道或者我们通过额外 API 查
        return Ok(new { complete = false, uploadedChunks = new int[0] });
    }

    // 2. 上传分片
    [HttpPost("chunk")]
    public async Task<IActionResult> UploadChunk([FromForm] IFormFile file, [FromForm] string hash,
        [FromForm] int index)
    {
        var (valid, msg) = await _validator.ValidateAsync(file);
        if (!valid) return BadRequest(msg);

        await using var stream = file.OpenReadStream();
        await _storage.SaveChunkAsync(stream, hash, index);
        return Ok();
    }

    // 3. 合并分片
    [HttpPost("merge")]
    public async Task<IActionResult> Merge([FromBody] FileMergeDto dto)
    {
        var (root, path) = await _storage.MergeChunksAsync(dto.Hash, dto.FileName);

        var file = new FileAttachment
        {
            Id = Guid.NewGuid(),
            OriginalName = dto.FileName,
            FileHash = dto.Hash,
            StorageName = Path.GetFileName(path),
            Extension = Path.GetExtension(dto.FileName),
            RootIdentifier = root,
            RelativePath = path,
            IsPermanent = false // 待业务表单提交后改为 true
        };

        await _fsql.Insert(file).ExecuteAffrowsAsync();
        return Ok(CreateDto(file));
    }

    private FileAttachmentDto CreateDto(FileAttachment file)
    {
        return new FileAttachmentDto
        {
            Id = file.Id,
            OriginalName = file.OriginalName,
            StorageName = file.StorageName,
            Extension = file.Extension,
            FileSize = file.FileSize,
            RelativePath = file.RelativePath,
            Url = _fileAccessSignatureService.CreatePreviewUrl(file.Id, file.AccessVersion),
            DownloadUrl = _fileAccessSignatureService.CreateDownloadUrl(file.Id, file.AccessVersion),
        };
    }

    private static string GetContentType(FileAttachment file)
    {
        return string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;
    }

    private async Task<FileAttachment?> ResolveFileAsync(string token, bool isPreview)
    {
        FileAccessPayload payload;
        var valid = isPreview
            ? _fileAccessSignatureService.TryValidatePreviewToken(token, out payload)
            : _fileAccessSignatureService.TryValidateDownloadToken(token, out payload);
        if (!valid)
        {
            return null;
        }

        var file = await _fsql.Select<FileAttachment>().Where(x => x.Id == payload.FileId).FirstAsync();
        if (file == null || file.AccessVersion != payload.AccessVersion)
        {
            return null;
        }

        return file;
    }

    private sealed record UploadProgressMessage(string UploadUid, int Percent);
}
