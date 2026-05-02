using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cits.Dtos;

namespace Cits.Dictionaries;

public interface IDataDictItemAppService : ICrudAppService<DataDictItemDto, Guid, QueryDataDictItemDto, DataDictItemDto, CreateDataDictItemDto, UpdateDataDictItemDto>
{
    Task<List<DataDictItemDto>> GetItemsByCodeAsync(string code);
}
