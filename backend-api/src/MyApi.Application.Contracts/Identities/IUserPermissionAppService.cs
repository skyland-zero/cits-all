using Cits.DI;
using Cits.Dtos;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface IUserPermissionAppService : IApplicationService
{
    Task<string[]> GetPermissionCodesAsync();

    Task<CurrentIdentityUserDto?> GetCurrentAsync();

    Task<List<IdentityUserMenusDto>> GetCurrentMenusAsync();

    Task<List<UserPcMenuDto>> GetPcMenusAsync();

    Task PreWarmCacheAsync(Guid userId);
}