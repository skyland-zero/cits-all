using System;
using Cits.Dtos;

namespace Cits.Dictionaries;

public interface IDataDictTypeAppService : ICrudAppService<DataDictTypeDto, Guid, QueryDataDictTypeDto, DataDictTypeDto, CreateDataDictTypeDto, UpdateDataDictTypeDto>
{
}
