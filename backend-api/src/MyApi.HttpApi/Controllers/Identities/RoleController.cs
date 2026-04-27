using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
public class RoleController : IdentityBaseApiController
{
    private readonly IRoleAppService _roleAppService;

    public RoleController(IRoleAppService roleAppService)
    {
        _roleAppService = roleAppService;
    }

    [HttpGet("{id}")]
    public async Task<RoleDto> GetAsync(Guid id)
    {
        return await _roleAppService.GetAsync(id);
    }

    [HttpGet]
    public async Task<PagedResultDto<RoleDto>> GetListAsync([FromQuery]GetRolesInput input)
    {
        return await _roleAppService.GetListAsync(input);
    }

    [HttpPost]
    public async Task CreateAsync(RoleCreateUpdateDto input)
    {
        await _roleAppService.CreateAsync(input);
    }

    [HttpPost("{id}/update")]
    public async Task UpdateAsync(Guid id, RoleCreateUpdateDto input)
    {
        await _roleAppService.UpdateAsync(id, input);
    }

    [HttpPost("{id}/delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _roleAppService.DeleteAsync(id);
    }

    [HttpGet("{id}/menu-ids")]
    public async Task<ListResultDto<Guid>> GetMenuIdsAsync(Guid id)
    {
        return await _roleAppService.GetMenuIdsAsync(id);
    }

    [HttpPost("{id}/menus")]
    public async Task UpdateMenusAsync(Guid id, RoleMenusUpdateDto input)
    {
        await _roleAppService.UpdateMenusAsync(id, input);
    }

    [HttpGet("select")]
    public async Task<ListResultDto<GuidSelectDto>> GetSelectAsync()
    {
        return await _roleAppService.GetSelectAsync();
    }
}
