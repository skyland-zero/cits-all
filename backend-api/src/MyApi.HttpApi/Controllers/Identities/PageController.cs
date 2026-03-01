using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
public class PageController : IdentityBaseApiController
{
    private readonly IIdentityPageAppService _identityPageAppService;


    public PageController(IIdentityPageAppService identityPageAppService)
    {
        _identityPageAppService = identityPageAppService;
    }

    [HttpGet("{id}")]
    public async Task<IdentityPageDto> GetAsync(Guid id)
    {
        return await _identityPageAppService.GetAsync(id);
    }

    [HttpGet]
    public async Task<PagedResultDto<IdentityPageDto>> GetListAsync([FromQuery]GetIdentityPagesInput input)
    {
        return await _identityPageAppService.GetListAsync(input);
    }

    [HttpPost]
    public async Task CreateAsync(IdentityPageCreateUpdateDto input)
    {
        await _identityPageAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public async Task UpdateAsync(Guid id, IdentityPageCreateUpdateDto input)
    {
        await _identityPageAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
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