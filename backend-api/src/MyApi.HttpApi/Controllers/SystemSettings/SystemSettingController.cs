using Cits;
using Cits.Dtos;
using Cits.SystemSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.HttpApi.Controllers.SystemSettings;

/// <summary>
/// 系统参数配置接口。
/// </summary>
[Route("api/system/settings")]
[Authorize]
public class SystemSettingController : BaseApiController
{
    private readonly ISystemSettingAppService _systemSettingAppService;

    public SystemSettingController(ISystemSettingAppService systemSettingAppService)
    {
        _systemSettingAppService = systemSettingAppService;
    }

    [HttpGet]
    public Task<PagedResultDto<SystemSettingDto>> GetListAsync([FromQuery] QuerySystemSettingDto input)
    {
        return _systemSettingAppService.GetListAsync(input);
    }

    [HttpGet("groups")]
    public Task<List<SystemSettingGroupDto>> GetGroupsAsync()
    {
        return _systemSettingAppService.GetGroupsAsync();
    }

    [HttpGet("{id:guid}")]
    public Task<SystemSettingDto> GetAsync(Guid id)
    {
        return _systemSettingAppService.GetAsync(id);
    }

    [HttpPost]
    public Task CreateAsync([FromBody] CreateSystemSettingDto input)
    {
        return _systemSettingAppService.CreateAsync(input);
    }

    [HttpPut("{id:guid}")]
    public Task UpdateAsync(Guid id, [FromBody] UpdateSystemSettingDto input)
    {
        return _systemSettingAppService.UpdateAsync(id, input);
    }

    [HttpPatch("{id:guid}/value")]
    public Task UpdateValueAsync(Guid id, [FromBody] UpdateSystemSettingValueDto input)
    {
        return _systemSettingAppService.UpdateValueAsync(id, input);
    }

    [HttpDelete("{id:guid}")]
    public Task DeleteAsync(Guid id)
    {
        return _systemSettingAppService.DeleteAsync(id);
    }
}
