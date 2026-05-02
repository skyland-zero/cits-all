using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cits;
using Cits.Dictionaries;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.HttpApi.Controllers.Dictionaries;

/// <summary>
/// 数据字典管理接口
/// </summary>
[Route("api/system/dict")]
[Authorize]
public class DictionaryController : BaseApiController
{
    private readonly IDataDictTypeAppService _dictTypeAppService;
    private readonly IDataDictItemAppService _dictItemAppService;

    public DictionaryController(IDataDictTypeAppService dictTypeAppService, IDataDictItemAppService dictItemAppService)
    {
        _dictTypeAppService = dictTypeAppService;
        _dictItemAppService = dictItemAppService;
    }

    // --- Dict Types ---

    [HttpGet("types")]
    public Task<PagedResultDto<DataDictTypeDto>> GetTypesAsync([FromQuery] QueryDataDictTypeDto input)
    {
        return _dictTypeAppService.GetListAsync(input);
    }

    [HttpGet("types/{id:guid}")]
    public Task<DataDictTypeDto> GetTypeAsync(Guid id)
    {
        return _dictTypeAppService.GetAsync(id);
    }

    [HttpPost("types")]
    public Task CreateTypeAsync([FromBody] CreateDataDictTypeDto input)
    {
        return _dictTypeAppService.CreateAsync(input);
    }

    [HttpPut("types/{id:guid}")]
    public Task UpdateTypeAsync(Guid id, [FromBody] UpdateDataDictTypeDto input)
    {
        return _dictTypeAppService.UpdateAsync(id, input);
    }

    [HttpDelete("types/{id:guid}")]
    public Task DeleteTypeAsync(Guid id)
    {
        return _dictTypeAppService.DeleteAsync(id);
    }

    // --- Dict Items ---

    [HttpGet("items")]
    public Task<PagedResultDto<DataDictItemDto>> GetItemsAsync([FromQuery] QueryDataDictItemDto input)
    {
        return _dictItemAppService.GetListAsync(input);
    }

    [HttpGet("items/{id:guid}")]
    public Task<DataDictItemDto> GetItemAsync(Guid id)
    {
        return _dictItemAppService.GetAsync(id);
    }

    [HttpPost("items")]
    public Task CreateItemAsync([FromBody] CreateDataDictItemDto input)
    {
        return _dictItemAppService.CreateAsync(input);
    }

    [HttpPut("items/{id:guid}")]
    public Task UpdateItemAsync(Guid id, [FromBody] UpdateDataDictItemDto input)
    {
        return _dictItemAppService.UpdateAsync(id, input);
    }

    [HttpDelete("items/{id:guid}")]
    public Task DeleteItemAsync(Guid id)
    {
        return _dictItemAppService.DeleteAsync(id);
    }

    /// <summary>
    /// 根据字典编码获取明细列表（用于前端组件下拉框等）
    /// </summary>
    [HttpGet("items/code/{code}")]
    [AllowAnonymous] // 取决于业务需求，有些系统允许未登录用户加载字典
    public Task<List<DataDictItemDto>> GetItemsByCodeAsync(string code)
    {
        return _dictItemAppService.GetItemsByCodeAsync(code);
    }
}
