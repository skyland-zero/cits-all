using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Mapster;
using MyApi.Application.FileManagement.Dto;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.FileUpload;

namespace MyApi.Application.FileManagement;

public class FileManagementAppService : IFileManagementAppService
{
    private readonly IFreeSql _freeSql;
    private readonly IStorageProvider _storageProvider;
    private readonly FileAccessSignatureService _fileAccessSignatureService;
    private readonly ICurrentUser _currentUser;

    public FileManagementAppService(
        IFreeSql freeSql,
        IStorageProvider storageProvider,
        FileAccessSignatureService fileAccessSignatureService,
        ICurrentUser currentUser)
    {
        _freeSql = freeSql;
        _storageProvider = storageProvider;
        _fileAccessSignatureService = fileAccessSignatureService;
        _currentUser = currentUser;
    }

    public async Task<PagedResultDto<FileManagementDto>> GetListAsync(QueryFileManagementDto input)
    {
        var keyword = input.Keyword ?? string.Empty;
        var extension = NormalizeExtension(input.Extension);
        var contentType = input.ContentType ?? string.Empty;

        var query = _freeSql.Select<FileAttachment>()
            .WhereIf(!keyword.IsNullOrWhiteSpace(), x =>
                x.OriginalName.Contains(keyword) ||
                x.StorageName.Contains(keyword) ||
                x.RelativePath.Contains(keyword))
            .WhereIf(!extension.IsNullOrWhiteSpace(), x => x.Extension == extension)
            .WhereIf(!contentType.IsNullOrWhiteSpace(), x => x.ContentType.Contains(contentType))
            .WhereIf(input.IsPermanent.HasValue, x => x.IsPermanent == input.IsPermanent)
            .WhereIf(input.StartTime.HasValue, x => x.CreateTime >= input.StartTime)
            .WhereIf(input.EndTime.HasValue, x => x.CreateTime <= input.EndTime);

        var count = await query.CountAsync();
        if (count == 0) return new PagedResultDto<FileManagementDto>(count, []);

        var list = await query
            .OrderByDescending(x => x.CreateTime)
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<FileManagementDto>(count, list.ConvertAll(ToDto));
    }

    public async Task<FileManagementDto> GetAsync(Guid id)
    {
        var entity = await _freeSql.Select<FileAttachment>().WhereDynamic(id).FirstAsync()
                     ?? throw new UserFriendlyException("文件不存在");
        return ToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _freeSql.Select<FileAttachment>().WhereDynamic(id).FirstAsync();
        if (entity == null) return;

        await DeletePhysicalAndRecordAsync(entity);
    }

    public async Task<FileCleanupResultDto> BatchDeleteAsync(BatchDeleteFileDto input)
    {
        var result = new FileCleanupResultDto();
        var ids = input.Ids.Distinct().ToList();
        if (ids.Count == 0) return result;

        var files = await _freeSql.Select<FileAttachment>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        foreach (var file in files)
        {
            await TryDeleteAsync(file, result);
        }

        return result;
    }

    public async Task<FileManagementDto> ReplaceAsync(Guid sourceFileId, Stream stream, string fileName, string contentType, long fileSize)
    {
        var source = await _freeSql.Select<FileAttachment>().WhereDynamic(sourceFileId).FirstAsync()
                     ?? throw new UserFriendlyException("源文件不存在");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var storageName = $"{Guid.NewGuid()}{extension}";
        var root = await _storageProvider.SaveAsync(stream, storageName, contentType, null);
        var relativePath = Path.Combine("business", storageName).Replace("\\", "/");

        var replacement = new FileAttachment
        {
            Id = Guid.NewGuid(),
            OriginalName = fileName,
            FileHash = string.Empty,
            StorageName = storageName,
            Extension = extension,
            FileSize = fileSize,
            ContentType = contentType,
            RootIdentifier = root,
            RelativePath = relativePath,
            CreateTime = DateTime.Now,
            IsPermanent = source.IsPermanent,
            CreatorUserId = _currentUser.Id,
            CreatorUserName = _currentUser.Surname,
            LastModifierUserId = _currentUser.Id,
            LastModifierUserName = _currentUser.Surname
        };

        var record = new FileReplacementRecord
        {
            Id = Guid.NewGuid(),
            SourceFileId = source.Id,
            ReplacementFileId = replacement.Id,
            SourceOriginalName = source.OriginalName,
            SourceRelativePath = source.RelativePath,
            SourceFileSize = source.FileSize,
            SourceExtension = source.Extension,
            SourceAccessVersion = source.AccessVersion,
            ReplacementOriginalName = replacement.OriginalName,
            ReplacementRelativePath = replacement.RelativePath,
            ReplacementFileSize = replacement.FileSize,
            ReplacementExtension = replacement.Extension,
            ReplacementAccessVersion = replacement.AccessVersion,
            ReplacedTime = DateTime.Now,
            ReplacedByUserId = _currentUser.Id,
            ReplacedByUserName = _currentUser.Surname,
            CreatorUserId = _currentUser.Id,
            CreatorUserName = _currentUser.Surname,
            LastModifierUserId = _currentUser.Id,
            LastModifierUserName = _currentUser.Surname
        };

        await _freeSql.Insert(replacement).ExecuteAffrowsAsync();
        await _freeSql.Insert(record).ExecuteAffrowsAsync();

        return ToDto(replacement);
    }

    public async Task<List<FileReplacementRecordDto>> GetReplacementRecordsAsync(Guid fileId)
    {
        var records = await _freeSql.Select<FileReplacementRecord>()
            .Where(x => x.SourceFileId == fileId || x.ReplacementFileId == fileId)
            .OrderByDescending(x => x.ReplacedTime)
            .ToListAsync();

        if (records.Count == 0) return [];

        var fileIds = records
            .SelectMany(x => new[] { x.SourceFileId, x.ReplacementFileId })
            .Distinct()
            .ToList();
        var fileList = await _freeSql.Select<FileAttachment>()
            .Where(x => fileIds.Contains(x.Id))
            .ToListAsync();
        var files = fileList.ToDictionary(x => x.Id);

        return records.ConvertAll(record => ToReplacementRecordDto(record, files));
    }

    public async Task<FileCleanupResultDto> CleanupTemporaryAsync(CleanupTemporaryFilesDto input)
    {
        var result = new FileCleanupResultDto();
        var hours = input.OlderThanHours <= 0 ? 24 : input.OlderThanHours;
        var cutoff = DateTime.Now.AddHours(-hours);
        var files = await _freeSql.Select<FileAttachment>()
            .Where(x => !x.IsPermanent && x.CreateTime < cutoff)
            .ToListAsync();

        foreach (var file in files)
        {
            await TryDeleteAsync(file, result);
        }

        return result;
    }

    private async Task TryDeleteAsync(FileAttachment file, FileCleanupResultDto result)
    {
        try
        {
            await DeletePhysicalAndRecordAsync(file);
            result.DeletedCount++;
        }
        catch (Exception ex)
        {
            result.FailedCount++;
            result.FailedMessages.Add($"{file.OriginalName}：{ex.Message}");
        }
    }

    private async Task DeletePhysicalAndRecordAsync(FileAttachment file)
    {
        await _storageProvider.DeleteAsync(file.RootIdentifier, file.RelativePath);
        await _freeSql.Delete<FileAttachment>().Where(x => x.Id == file.Id).ExecuteAffrowsAsync();
    }

    private FileManagementDto ToDto(FileAttachment entity)
    {
        var dto = entity.Adapt<FileManagementDto>();
        dto.PreviewUrl = _fileAccessSignatureService.CreatePreviewUrl(entity.Id, entity.AccessVersion);
        dto.DownloadUrl = _fileAccessSignatureService.CreateDownloadUrl(entity.Id, entity.AccessVersion);
        return dto;
    }

    private FileReplacementRecordDto ToReplacementRecordDto(
        FileReplacementRecord record,
        Dictionary<Guid, FileAttachment> files)
    {
        var dto = record.Adapt<FileReplacementRecordDto>();

        if (files.TryGetValue(record.SourceFileId, out var sourceFile))
        {
            dto.SourcePreviewUrl = _fileAccessSignatureService.CreatePreviewUrl(sourceFile.Id, sourceFile.AccessVersion);
            dto.SourceDownloadUrl = _fileAccessSignatureService.CreateDownloadUrl(sourceFile.Id, sourceFile.AccessVersion);
        }
        else
        {
            dto.SourcePreviewUrl = _fileAccessSignatureService.CreatePreviewUrl(record.SourceFileId, record.SourceAccessVersion);
            dto.SourceDownloadUrl = _fileAccessSignatureService.CreateDownloadUrl(record.SourceFileId, record.SourceAccessVersion);
        }

        if (files.TryGetValue(record.ReplacementFileId, out var replacementFile))
        {
            dto.ReplacementPreviewUrl = _fileAccessSignatureService.CreatePreviewUrl(replacementFile.Id, replacementFile.AccessVersion);
            dto.ReplacementDownloadUrl = _fileAccessSignatureService.CreateDownloadUrl(replacementFile.Id, replacementFile.AccessVersion);
        }
        else
        {
            dto.ReplacementPreviewUrl = _fileAccessSignatureService.CreatePreviewUrl(record.ReplacementFileId, record.ReplacementAccessVersion);
            dto.ReplacementDownloadUrl = _fileAccessSignatureService.CreateDownloadUrl(record.ReplacementFileId, record.ReplacementAccessVersion);
        }

        return dto;
    }

    private static string? NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return extension;
        return extension.StartsWith('.') ? extension : $".{extension}";
    }
}
