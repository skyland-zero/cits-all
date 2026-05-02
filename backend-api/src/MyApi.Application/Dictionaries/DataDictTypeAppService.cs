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

namespace MyApi.Application.Dictionaries;

public class DataDictTypeAppService : IDataDictTypeAppService
{
    private readonly IFreeSql _freeSql;
    private readonly IIdGenerator _idGenerator;

    public DataDictTypeAppService(IFreeSql freeSql, IIdGenerator idGenerator)
    {
        _freeSql = freeSql;
        _idGenerator = idGenerator;
    }

    public async Task<DataDictTypeDto> GetAsync(Guid id)
    {
        var entity = await _freeSql.Select<DataDictType>().Where(x => x.Id == id).FirstAsync();
        return entity.Adapt<DataDictTypeDto>();
    }

    public async Task<PagedResultDto<DataDictTypeDto>> GetListAsync(QueryDataDictTypeDto input)
    {
        var query = _freeSql.Select<DataDictType>()
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), x => x.Code.Contains(input.Keyword!) || x.Name.Contains(input.Keyword!));

        var count = await query.CountAsync();
        if (count == 0) return new PagedResultDto<DataDictTypeDto>();

        var list = await query.OrderByDescending(x => x.CreationTime).PageBy(input).ToListAsync<DataDictTypeDto>();
        return new PagedResultDto<DataDictTypeDto>(count, list);
    }

    public async Task CreateAsync(CreateDataDictTypeDto input)
    {
        var entity = input.Adapt<DataDictType>();
        entity.Id = _idGenerator.Create();
        await _freeSql.Insert(entity).ExecuteAffrowsAsync();
    }

    public async Task UpdateAsync(Guid id, UpdateDataDictTypeDto input)
    {
        var entity = await _freeSql.Select<DataDictType>().Where(x => x.Id == id).FirstAsync() 
                     ?? throw new Exception("字典分类不存在");

        entity.Name = input.Name;
        entity.Description = input.Description;

        await _freeSql.Update<DataDictType>().SetSource(entity).ExecuteAffrowsAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _freeSql.Delete<DataDictType>().Where(x => x.Id == id).ExecuteAffrowsAsync();
        // 软删除也可以考虑用Update
    }
}
