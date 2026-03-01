using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
public class OrganizationUnitController : IdentityBaseApiController
{
    private readonly IOrganizationUnitService _organizationUnitService;

    public OrganizationUnitController(IOrganizationUnitService organizationUnitService)
    {
        _organizationUnitService = organizationUnitService;
    }

    [HttpGet("{id}")]
    public async Task<OrganizationUnitDto> GetAsync(Guid id)
    {
        return await _organizationUnitService.GetAsync(id);
    }

    [HttpGet]
    public async Task<PagedResultDto<OrganizationUnitDto>> GetListAsync([FromQuery]GetOrganizationUnitInput input)
    {
        return await _organizationUnitService.GetListAsync(input);
    }

    [HttpPost]
    public async Task CreateAsync(OrganizationUnitCreateUpdateDto input)
    {
        await _organizationUnitService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public async Task UpdateAsync(Guid id, OrganizationUnitCreateUpdateDto input)
    {
        await _organizationUnitService.UpdateAsync(id, input);
    }

    [HttpDelete]
    public async Task DeleteAsync(Guid id)
    {
        await _organizationUnitService.DeleteAsync(id);
    }

    [HttpGet("tree")]
    public async Task<ListResultDto<OrganizationUnitTreeDto>> GetTreeAsync()
    {
        return await _organizationUnitService.GetTreeAsync();
    }
}