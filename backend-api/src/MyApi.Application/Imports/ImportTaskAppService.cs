using Cits;
using Cits.Dtos;
using Cits.Extensions;
using Microsoft.Extensions.Configuration;
using MyApi.Application.Imports.Dto;
using MyApi.Domain.DomainServices.FileUpload;
using MyApi.Domain.Imports;
using MyApi.Domain.Shared.Imports;

namespace MyApi.Application.Imports;

public class ImportTaskAppService : IImportTaskAppService
{
    private readonly IConfiguration _configuration;
    private readonly ICurrentUser _currentUser;
    private readonly IFreeSql _freeSql;
    private readonly IReadOnlyDictionary<string, IImportProvider> _providers;
    private readonly IStorageProvider _storageProvider;

    public ImportTaskAppService(
        IFreeSql freeSql,
        IEnumerable<IImportProvider> providers,
        IStorageProvider storageProvider,
        ICurrentUser currentUser,
        IConfiguration configuration)
    {
        _freeSql = freeSql;
        _storageProvider = storageProvider;
        _currentUser = currentUser;
        _configuration = configuration;
        _providers = providers.ToDictionary(x => x.ModuleKey, StringComparer.OrdinalIgnoreCase);
    }

    public Task<ListResultDto<ImportModuleDto>> GetModulesAsync()
    {
        var modules = _providers.Values
            .Select(x => new ImportModuleDto { ModuleKey = x.ModuleKey, ModuleName = x.ModuleName })
            .OrderBy(x => x.ModuleName)
            .ToList();
        return Task.FromResult(new ListResultDto<ImportModuleDto>(modules));
    }

    public Task<(byte[] Bytes, string FileName)> BuildTemplateAsync(string moduleKey)
    {
        var provider = GetProvider(moduleKey);
        var bytes = XlsxImportTemplateBuilder.Build(provider.Columns);
        return Task.FromResult((bytes, $"{provider.ModuleName}导入模板.xlsx"));
    }

    public async Task<ImportTaskDto> CreateAsync(string moduleKey, Stream stream, string fileName, string contentType, long fileSize)
    {
        var provider = GetProvider(moduleKey);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("仅支持 xlsx 文件导入");
        }

        var now = DateTime.Now;
        var storageName = $"import_{Guid.NewGuid():N}.xlsx";
        var root = await _storageProvider.SaveAsync(stream, storageName, contentType, null);
        var businessSubFolder = _configuration["StorageConfig:BusinessSubFolder"] ?? "business";
        var task = new ImportTask
        {
            Id = Guid.NewGuid(),
            ModuleKey = provider.ModuleKey,
            ModuleName = provider.ModuleName,
            OriginalFileName = fileName,
            StorageName = storageName,
            RootIdentifier = root,
            RelativePath = Path.Combine(businessSubFolder, storageName).Replace("\\", "/"),
            ContentType = string.IsNullOrWhiteSpace(contentType)
                ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                : contentType,
            FileSize = fileSize,
            Status = ImportTaskStatus.Pending,
            CreationTime = now,
            LastModificationTime = now,
            CreatorUserId = _currentUser.Id,
            CreatorUserName = _currentUser.UserName,
            LastModifierUserId = _currentUser.Id,
            LastModifierUserName = _currentUser.UserName
        };

        await _freeSql.Insert(task).ExecuteAffrowsAsync();
        return ToDto(task);
    }

    public async Task<PagedResultDto<ImportTaskDto>> GetListAsync(GetImportTasksInput input)
    {
        var query = _freeSql.Select<ImportTask>()
            .WhereIf(_currentUser.Id != Guid.Empty, x => x.CreatorUserId == _currentUser.Id)
            .WhereIf(!input.ModuleKey.IsNullOrWhiteSpace(), x => x.ModuleKey == input.ModuleKey);

        var count = await query.CountAsync();
        if (count == 0) return new PagedResultDto<ImportTaskDto>(0, []);

        var list = await query
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input)
            .ToListAsync();
        return new PagedResultDto<ImportTaskDto>(count, list.ConvertAll(ToDto));
    }

    private IImportProvider GetProvider(string moduleKey)
    {
        if (!_providers.TryGetValue(moduleKey, out var provider))
        {
            throw new UserFriendlyException("导入模块未注册");
        }

        return provider;
    }

    public static ImportTaskDto ToDto(ImportTask task)
    {
        return new ImportTaskDto
        {
            Id = task.Id,
            ModuleKey = task.ModuleKey,
            ModuleName = task.ModuleName,
            OriginalFileName = task.OriginalFileName,
            Status = task.Status,
            TotalCount = task.TotalCount,
            SuccessCount = task.SuccessCount,
            FailedCount = task.FailedCount,
            FileSize = task.FileSize,
            CreationTime = task.CreationTime,
            StartedTime = task.StartedTime,
            FinishedTime = task.FinishedTime,
            ErrorMessage = task.ErrorMessage,
            CanDownloadErrorReport = task.FailedCount > 0 && !task.ErrorReportRelativePath.IsNullOrWhiteSpace()
        };
    }
}
