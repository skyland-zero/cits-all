using System.Globalization;
using System.Text.Json;
using Cits;
using Cits.Domain.SystemSettings;
using Cits.Dtos;
using Cits.Extensions;
using Cits.IdGenerator;
using Cits.SystemSettings;
using Mapster;
using Microsoft.Extensions.Caching.Hybrid;

namespace MyApi.Application.SystemSettings;

public class SystemSettingAppService : ISystemSettingAppService, ISettingProvider
{
    private const string CacheKeyPrefix = "SystemSetting:";
    private readonly IFreeSql _freeSql;
    private readonly HybridCache _cache;
    private readonly IIdGenerator _idGenerator;

    public SystemSettingAppService(IFreeSql freeSql, HybridCache cache, IIdGenerator idGenerator)
    {
        _freeSql = freeSql;
        _cache = cache;
        _idGenerator = idGenerator;
    }

    public async Task<SystemSettingDto> GetAsync(Guid id)
    {
        var entity = await _freeSql.Select<SystemSetting>().WhereDynamic(id).FirstAsync()
                     ?? throw new UserFriendlyException("系统参数不存在");
        return entity.Adapt<SystemSettingDto>();
    }

    public async Task<PagedResultDto<SystemSettingDto>> GetListAsync(QuerySystemSettingDto input)
    {
        var keyword = input.Keyword ?? string.Empty;
        var group = input.Group ?? string.Empty;

        var query = _freeSql.Select<SystemSetting>()
            .WhereIf(!keyword.IsNullOrWhiteSpace(), x =>
                x.Key.Contains(keyword) || x.Name.Contains(keyword) ||
                (x.Description != null && x.Description.Contains(keyword)))
            .WhereIf(!group.IsNullOrWhiteSpace(), x => x.Group == group);

        var count = await query.CountAsync();
        if (count == 0) return new PagedResultDto<SystemSettingDto>(count, []);

        var list = await query.OrderBy(x => x.Group).OrderBy(x => x.Sort).OrderBy(x => x.Key)
            .PageBy(input)
            .ToListAsync<SystemSettingDto>();
        return new PagedResultDto<SystemSettingDto>(count, list);
    }

    public Task<List<SystemSettingGroupDto>> GetGroupsAsync()
    {
        var groups = new List<SystemSettingGroupDto>
        {
            new() { Value = SystemSettingGroups.Basic, Label = "基础配置" },
            new() { Value = SystemSettingGroups.Security, Label = "安全策略" },
            new() { Value = SystemSettingGroups.Upload, Label = "上传配置" },
            new() { Value = SystemSettingGroups.Import, Label = "导入配置" },
            new() { Value = SystemSettingGroups.Announcement, Label = "公告配置" }
        };
        return Task.FromResult(groups);
    }

    public async Task CreateAsync(CreateSystemSettingDto input)
    {
        await ValidateInputAsync(input.Key, input.ValueType, input.Value);

        if (await _freeSql.Select<SystemSetting>().AnyAsync(x => x.Key == input.Key))
        {
            throw new UserFriendlyException($"系统参数 {input.Key} 已存在");
        }

        var entity = input.Adapt<SystemSetting>();
        entity.Id = _idGenerator.Create();
        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
        await RemoveCacheAsync(entity.Key);
    }

    public async Task UpdateAsync(Guid id, UpdateSystemSettingDto input)
    {
        var entity = await _freeSql.Select<SystemSetting>().WhereDynamic(id).FirstAsync()
                     ?? throw new UserFriendlyException("系统参数不存在");

        await ValidateInputAsync(entity.Key, input.ValueType, input.Value);

        entity.Name = input.Name;
        entity.Value = input.Value;
        entity.ValueType = input.ValueType;
        entity.Group = input.Group;
        entity.Description = input.Description;
        entity.IsEncrypted = input.IsEncrypted;
        entity.IsReadonly = input.IsReadonly;
        entity.Sort = input.Sort;

        await _freeSql.Update<SystemSetting>().SetSource(entity).ExecuteAffrowsAsync();
        await RemoveCacheAsync(entity.Key);
    }

    public async Task UpdateValueAsync(Guid id, UpdateSystemSettingValueDto input)
    {
        var entity = await _freeSql.Select<SystemSetting>().WhereDynamic(id).FirstAsync()
                     ?? throw new UserFriendlyException("系统参数不存在");

        await ValidateInputAsync(entity.Key, entity.ValueType, input.Value);
        entity.Value = input.Value;

        await _freeSql.Update<SystemSetting>().SetSource(entity).ExecuteAffrowsAsync();
        await RemoveCacheAsync(entity.Key);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _freeSql.Select<SystemSetting>().WhereDynamic(id).FirstAsync();
        if (entity == null) return;

        if (entity.IsReadonly)
        {
            throw new UserFriendlyException("只读系统参数不允许删除");
        }

        await _freeSql.Delete<SystemSetting>().WhereDynamic(id).ExecuteAffrowsAsync();
        await RemoveCacheAsync(entity.Key);
    }

    public async Task<string?> GetStringAsync(string key, string? defaultValue = null)
    {
        var value = await GetCachedValueAsync(key);
        return value ?? defaultValue;
    }

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
    {
        var value = await GetCachedValueAsync(key);
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<int> GetIntAsync(string key, int defaultValue = 0)
    {
        var value = await GetCachedValueAsync(key);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue = 0)
    {
        var value = await GetCachedValueAsync(key);
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    private async Task<string?> GetCachedValueAsync(string key)
    {
        return await _cache.GetOrCreateAsync(
            BuildCacheKey(key),
            async _ => await _freeSql.Select<SystemSetting>()
                .Where(x => x.Key == key)
                .FirstAsync(x => x.Value),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            });
    }

    private Task RemoveCacheAsync(string key)
    {
        return _cache.RemoveAsync(BuildCacheKey(key)).AsTask();
    }

    private static string BuildCacheKey(string key) => $"{CacheKeyPrefix}{key}";

    private static Task ValidateInputAsync(string key, string valueType, string? value)
    {
        if (!SystemSettingValueTypes.All.Contains(valueType))
        {
            throw new UserFriendlyException("系统参数类型无效");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        switch (valueType)
        {
            case SystemSettingValueTypes.Number:
                if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                {
                    throw new UserFriendlyException($"系统参数 {key} 必须是数字");
                }
                break;
            case SystemSettingValueTypes.Boolean:
                if (!bool.TryParse(value, out _))
                {
                    throw new UserFriendlyException($"系统参数 {key} 必须是布尔值 true/false");
                }
                break;
            case SystemSettingValueTypes.Json:
                try
                {
                    JsonDocument.Parse(value);
                }
                catch (JsonException)
                {
                    throw new UserFriendlyException($"系统参数 {key} 必须是合法 JSON");
                }
                break;
        }

        return Task.CompletedTask;
    }
}
