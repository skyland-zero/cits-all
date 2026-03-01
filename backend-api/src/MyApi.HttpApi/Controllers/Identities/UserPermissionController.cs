using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
public class UserPermissionController : IdentityBaseApiController
{
    private readonly IUserPermissionAppService _userPermissionAppService;

    public UserPermissionController(IUserPermissionAppService userPermissionAppService)
    {
        _userPermissionAppService = userPermissionAppService;
    }

    [HttpGet("permission-codes")]
    public async Task<string[]> GetPermissionCodesAsync()
    {
        return await _userPermissionAppService.GetPermissionCodesAsync();
    }

    [HttpGet("current")]
    public async Task<CurrentIdentityUserDto?> GetCurrentAsync()
    {
        return await _userPermissionAppService.GetCurrentAsync();
    }
    
    [HttpGet("current-menus")]
    public async Task<List<IdentityUserMenusDto>> GetCurrentMenusAsync()
    {
        return await _userPermissionAppService.GetCurrentMenusAsync();
    }

    [HttpGet("pc-menus")]
    public async Task<List<UserPcMenuDto>> GetPcMenusAsync()
    {
        return await _userPermissionAppService.GetPcMenusAsync();
    }
}