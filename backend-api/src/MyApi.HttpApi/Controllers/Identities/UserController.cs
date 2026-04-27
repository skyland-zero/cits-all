using Cits;
using Cits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApi.Application.Identities;
using MyApi.Application.Identities.Dto;

namespace MyApi.HttpApi.Controllers.Identities;

[Authorize]
public class UserController : IdentityBaseApiController
{
    private readonly IUserAppService _userAppService;

    public UserController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpGet("{id}")]
    public async Task<UserDto> GetAsync(Guid id)
    {
        return await _userAppService.GetAsync(id);
    }

    [HttpGet]
    public async Task<PagedResultDto<UserDto>> GetListAsync([FromQuery] GetUsersInput input)
    {
        return await _userAppService.GetListAsync(input);
    }

    [HttpPost]
    public async Task CreateAsync(UserCreateDto input)
    {
        await _userAppService.CreateAsync(input);
    }

    [HttpPost("{id}/update")]
    public async Task UpdateAsync(Guid id, UserUpdateDto input)
    {
        await _userAppService.UpdateAsync(id, input);
    }

    [HttpPost("{id}/delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _userAppService.DeleteAsync(id);
    }

    [HttpPost("reset-password/{id}")]
    public async Task ResetPasswordAsync(Guid id, UserResetPasswordDto input)
    {
        await _userAppService.ResetPasswordAsync(id, input);
    }
}
