using Cits;
using Cits.Dtos;
using Cits.OperationLogs;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[OperationLog(OperationLogModules.Permission)]
public class PermissionController : IdentityBaseApiController
{
    private readonly IPermissionAppService _permissionAppService;

    public PermissionController(IPermissionAppService permissionAppService)
    {
        _permissionAppService = permissionAppService;
    }

    [HttpGet("tree-select")]
    [OperationLog(OperationType = OperationLogActions.List)]
    public Task<ListResultDto<TreeSelectStringDto>> GetTreeSelectAsync()
    {
        return _permissionAppService.GetTreeSelectAsync();
    }
    
    
}