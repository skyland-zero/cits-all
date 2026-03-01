using Cits;
using Cits.Dtos;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface IRoleAppService : ICrudAppService<
    RoleDto,
    Guid,
    GetRolesInput,
    RoleDto,
    RoleCreateUpdateDto,
    RoleCreateUpdateDto>
{
    Task<ListResultDto<Guid>> GetMenuIdsAsync(Guid id);

    Task UpdateMenusAsync(Guid id, RoleMenusUpdateDto input);

    Task<ListResultDto<GuidSelectDto>> GetSelectAsync();
}