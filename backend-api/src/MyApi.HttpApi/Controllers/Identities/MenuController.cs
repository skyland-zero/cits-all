using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
public class MenuController : IdentityBaseApiController
{
    private readonly IIdentityMenuAppService _identityMenuAppService;

    public MenuController(IIdentityMenuAppService identityMenuAppService)
    {
        _identityMenuAppService = identityMenuAppService;
    }

    [HttpGet("{id}")]
    public async Task<IdentityMenuDto> GetAsync(Guid id)
    {
        return await _identityMenuAppService.GetAsync(id);
    }

    [HttpGet]
    public async Task<PagedResultDto<IdentityMenuDto>> GetListAsync([FromQuery]GetIdentityMenusInput input)
    {
        return await _identityMenuAppService.GetListAsync(input);
    }

    [HttpGet("lite-list")]
    public async Task<PagedResultDto<IdentityMenuLiteDto>> GetLiteListAsync([FromQuery]GetIdentityMenusInput input)
    {
        return await _identityMenuAppService.GetLiteListAsync(input);
    }

    [HttpPost]
    public async Task CreateAsync(IdentityMenuCreateDto input)
    {
        await _identityMenuAppService.CreateAsync(input);
    }

    [HttpPost("multi-create")]
    public async Task MultiCreateAsync(List<IdentityMenuCreateDto> inputs)
    {
        await _identityMenuAppService.MultiCreateAsync(inputs);
    }

    [HttpPost("{id}/update")]
    public async Task UpdateAsync(Guid id, IdentityMenuUpdateDto input)
    {
        await _identityMenuAppService.UpdateAsync(id, input);
    }

    [HttpPost("{id}/delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _identityMenuAppService.DeleteAsync(id);
    }

    [HttpGet("tree")]
    public async Task<ListResultDto<IdentityMenuTreeDto>> GetTreeAsync()
    {
        return await _identityMenuAppService.GetTreeAsync();
    }

    [HttpGet("tree-select")]
    public async Task<ListResultDto<TreeSelectGuidDto>> GetTreeSelectAsync()
    {
        return await _identityMenuAppService.GetTreeSelectAsync();
    }
}
