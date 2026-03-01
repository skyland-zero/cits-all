using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

public class PermissionController : IdentityBaseApiController
{
    private readonly IPermissionAppService _permissionAppService;

    public PermissionController(IPermissionAppService permissionAppService)
    {
        _permissionAppService = permissionAppService;
    }

    [HttpGet("tree-select")]
    public Task<ListResultDto<TreeSelectStringDto>> GetTreeSelectAsync()
    {
        return _permissionAppService.GetTreeSelectAsync();
    }
    
    
}