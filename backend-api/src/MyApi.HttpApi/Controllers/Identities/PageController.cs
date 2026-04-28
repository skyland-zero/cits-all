using Cits;
using Cits.Dtos;
using Cits.OperationLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
[OperationLog(OperationLogModules.Page)]
public class PageController : IdentityBaseApiController
{
    private readonly IIdentityPageAppService _identityPageAppService;


    public PageController(IIdentityPageAppService identityPageAppService)
    {
        _identityPageAppService = identityPageAppService;
    }

    [HttpGet("{id}")]
    [OperationLog(OperationType = OperationLogActions.Detail)]
    public async Task<IdentityPageDto> GetAsync(Guid id)
    {
        return await _identityPageAppService.GetAsync(id);
    }

    [HttpGet]
    [OperationLog(OperationType = OperationLogActions.List)]
    public async Task<PagedResultDto<IdentityPageDto>> GetListAsync([FromQuery]GetIdentityPagesInput input)
    {
        return await _identityPageAppService.GetListAsync(input);
    }

    [HttpPost]
    public async Task CreateAsync(IdentityPageCreateUpdateDto input)
    {
        await _identityPageAppService.CreateAsync(input);
    }

    [HttpPost("{id}/update")]
    public async Task UpdateAsync(Guid id, IdentityPageCreateUpdateDto input)
    {
        await _identityPageAppService.UpdateAsync(id, input);
    }

    [HttpPost("{id}/delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _identityPageAppService.DeleteAsync(id);
    }

    [HttpGet("tree")]
    public async Task<ListResultDto<IdentityPageTreeDto>> GetTreeAsync()
    {
        return await _identityPageAppService.GetTreeAsync();
    }

    [HttpGet("tree-select")]
    public async Task<ListResultDto<TreeSelectGuidDto>> GetTreeSelectAsync()
    {
        return await _identityPageAppService.GetTreeSelectAsync();
    }
}
