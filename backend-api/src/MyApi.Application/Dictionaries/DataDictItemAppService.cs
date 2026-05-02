using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cits;
using Cits.Dictionaries;
using Cits.Domain.Dictionaries;
using Cits.Dtos;
using Cits.Extensions;
using Cits.IdGenerator;
using FreeSql;
using Mapster;
using Microsoft.Extensions.Caching.Hybrid;

namespace MyApi.Application.Dictionaries;

public class DataDictItemAppService : IDataDictItemAppService
{
    private readonly IFreeSql _freeSql;
    private readonly IIdGenerator _idGenerator;
    private readonly HybridCache _cache;
    private const string CacheKeyPrefix = "DictItems:";

    public DataDictItemAppService(IFreeSql freeSql, IIdGenerator idGenerator, HybridCache cache)
    {
        _freeSql = freeSql;
        _idGenerator = idGenerator;
        _cache = cache;
    }

    public async Task<DataDictItemDto> GetAsync(Guid id)
    {
        var entity = await _freeSql.Select<DataDictItem>().Where(x => x.Id == id).FirstAsync();
        return entity.Adapt<DataDictItemDto>();
    }

    public async Task<PagedResultDto<DataDictItemDto>> GetListAsync(QueryDataDictItemDto input)
    {
        var query = _freeSql.Select<DataDictItem>()
            .WhereIf(input.DictTypeId.HasValue, x => x.DictTypeId == input.DictTypeId);

        if (!string.IsNullOrWhiteSpace(input.DictCode))
        {
            var dictType = await _freeSql.Select<DataDictType>().Where(x => x.Code == input.DictCode).FirstAsync();
            if (dictType != null)
            {
                query = query.Where(x => x.DictTypeId == dictType.Id);
            }
            else
            {
                return new PagedResultDto<DataDictItemDto>();
            }
        }

        var count = await query.CountAsync();
        if (count == 0) return new PagedResultDto<DataDictItemDto>();

        var list = await query.OrderBy(x => x.Sort).PageBy(input).ToListAsync<DataDictItemDto>();
        return new PagedResultDto<DataDictItemDto>(count, list);
    }

    public async Task CreateAsync(CreateDataDictItemDto input)
    {
        var entity = input.Adapt<DataDictItem>();
        entity.Id = _idGenerator.Create();
        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
        await ClearCacheAsync(entity.DictTypeId);
    }

    public async Task UpdateAsync(Guid id, UpdateDataDictItemDto input)
    {
        var entity = await _freeSql.Select<DataDictItem>().Where(x => x.Id == id).FirstAsync() 
                     ?? throw new Exception("字典项不存在");

        entity.Label = input.Label;
        entity.Value = input.Value;
        entity.Sort = input.Sort;
        entity.IsEnabled = input.IsEnabled;

        await _freeSql.Update<DataDictItem>().SetSource(entity).ExecuteAffrowsAsync();
        await ClearCacheAsync(entity.DictTypeId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _freeSql.Select<DataDictItem>().Where(x => x.Id == id).FirstAsync();
        if (entity != null)
        {
            await _freeSql.Delete<DataDictItem>().Where(x => x.Id == id).ExecuteAffrowsAsync();
            await ClearCacheAsync(entity.DictTypeId);
        }
    }

    public async Task<List<DataDictItemDto>> GetItemsByCodeAsync(string code)
    {
        var cacheKey = $"{CacheKeyPrefix}{code}";
        var dictType = await _freeSql.Select<DataDictType>().Where(x => x.Code == code).FirstAsync();
        if (dictType == null) return new List<DataDictItemDto>();

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ => await _freeSql.Select<DataDictItem>()
                .Where(x => x.DictTypeId == dictType.Id && x.IsEnabled)
                .OrderBy(x => x.Sort)
                .ToListAsync<DataDictItemDto>(),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24)
            });
    }

    private async Task ClearCacheAsync(Guid dictTypeId)
    {
        var dictType = await _freeSql.Select<DataDictType>().Where(x => x.Id == dictTypeId).FirstAsync();
        if (dictType != null)
        {
            await _cache.RemoveAsync($"{CacheKeyPrefix}{dictType.Code}");
        }
    }
}
