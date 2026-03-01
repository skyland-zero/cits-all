using Cits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.DomainServices.FileUpload.Dto;
using MyApi.Domain.FileUpload;
using MyApi.HttpApi.Hubs;

namespace MyApi.HttpApi.Controllers.FileUpload;

[Route("api/basic/upload/[controller]")]
public class UploadController : BaseApiController
{
    private readonly IStorageProvider _storage;
    private readonly IFreeSql _fsql;
    private readonly FileValidationService _validator;
    private readonly IHubContext<UploadHub> _hub;

    public UploadController(IStorageProvider storage, IFreeSql fsql, FileValidationService validator,
        IHubContext<UploadHub> hub)
    {
        _storage = storage;
        _fsql = fsql;
        _validator = validator;
        _hub = hub;
    }

    /// <summary>
    /// 普通上传接口（支持 SignalR 进度）
    /// </summary>
    /// <param name="file">文件对象</param>
    /// <param name="connectionId">SignalR 连接 ID (从 Header 获取)</param>
    [HttpPost("single")]
    public async Task UploadSingle(IFormFile file,
        [FromHeader(Name = "X-Connection-Id")] string connectionId)
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
                if (!string.IsNullOrEmpty(connectionId))
                {
                    // Fire-and-forget: 不要 await，避免阻塞文件写入流
                    _ = _hub.Clients.Client(connectionId).SendAsync("UploadProgress", percent);
                }
            });
        }
        catch (Exception ex)
        {
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
        return Ok(file);
    }
}