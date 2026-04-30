using System.Text.Json;
using Cits;
using Cits.Dtos;
using Cits.Extensions;
using MyApi.Application.Exports.Dto;
using MyApi.Domain.Exports;
using MyApi.Domain.Shared.Exports;

namespace MyApi.Application.Exports;

public class ExportTaskAppService : IExportTaskAppService
{
    private readonly ICurrentUser _currentUser;
    private readonly IFreeSql _freeSql;
    private readonly IReadOnlyDictionary<string, IExportProvider> _providers;

    public ExportTaskAppService(
        IFreeSql freeSql,
        IEnumerable<IExportProvider> providers,
        ICurrentUser currentUser)
    {
        _freeSql = freeSql;
        _currentUser = currentUser;
        _providers = providers.ToDictionary(x => x.ModuleKey, StringComparer.OrdinalIgnoreCase);
    }

    public Task<ListResultDto<ExportFieldDto>> GetFieldsAsync(string moduleKey)
    {
        var provider = GetProvider(moduleKey);
        return Task.FromResult(new ListResultDto<ExportFieldDto>(provider.Fields.ToList()));
    }

    public async Task<ExportTaskDto> CreateAsync(CreateExportTaskInput input)
    {
        if (input.ModuleKey.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException("导出模块不能为空");
        }

        var provider = GetProvider(input.ModuleKey);
        var fieldKeys = NormalizeFields(provider, input.Fields);
        var now = DateTime.Now;
        var fileName = BuildFileName(input.FileName, provider.ModuleName, now);
        var task = new ExportTask
        {
            Id = Guid.NewGuid(),
            ModuleKey = provider.ModuleKey,
            ModuleName = provider.ModuleName,
            FileName = fileName,
            QueryJson = JsonSerializer.Serialize(input.Query),
            FieldsJson = JsonSerializer.Serialize(fieldKeys),
            Status = ExportTaskStatus.Pending,
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

    public async Task<PagedResultDto<ExportTaskDto>> GetListAsync(GetExportTasksInput input)
    {
        var query = _freeSql.Select<ExportTask>()
            .WhereIf(_currentUser.Id != Guid.Empty, x => x.CreatorUserId == _currentUser.Id)
            .WhereIf(!input.ModuleKey.IsNullOrWhiteSpace(), x => x.ModuleKey == input.ModuleKey);

        var count = await query.CountAsync();
        if (count == 0)
        {
            return new PagedResultDto<ExportTaskDto>(0, []);
        }

        var list = await query
            .OrderByDescending(x => x.CreationTime)
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<ExportTaskDto>(count, list.Select(ToDto).ToList());
    }

    private IExportProvider GetProvider(string moduleKey)
    {
        if (!_providers.TryGetValue(moduleKey, out var provider))
        {
            throw new UserFriendlyException("导出模块未注册");
        }

        return provider;
    }

    private static List<string> NormalizeFields(IExportProvider provider, IReadOnlyCollection<string> requestedFields)
    {
        var fieldMap = provider.Fields.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        var selected = requestedFields.Count > 0
            ? requestedFields.Where(fieldMap.ContainsKey).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : provider.Fields.Where(x => x.Selected).Select(x => x.Key).ToList();

        if (selected.Count == 0)
        {
            throw new UserFriendlyException("至少选择一个导出字段");
        }

        return selected;
    }

    private static string BuildFileName(string? inputName, string moduleName, DateTime now)
    {
        var name = inputName.IsNullOrWhiteSpace() ? $"{moduleName}导出" : inputName!.Trim();
        name = Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, '_'));
        return $"{name}_{now:yyyyMMddHHmmss}.csv";
    }

    private static ExportTaskDto ToDto(ExportTask task)
    {
        return new ExportTaskDto
        {
            Id = task.Id,
            ModuleKey = task.ModuleKey,
            ModuleName = task.ModuleName,
            FileName = task.FileName,
            Status = task.Status,
            TotalCount = task.TotalCount,
            FileSize = task.FileSize,
            CreationTime = task.CreationTime,
            StartedTime = task.StartedTime,
            FinishedTime = task.FinishedTime,
            ErrorMessage = task.ErrorMessage,
            CanDownload = task.Status == ExportTaskStatus.Succeeded && !task.RelativePath.IsNullOrWhiteSpace()
        };
    }
}
