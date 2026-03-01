using Cits;
using Cits.Dtos;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface IIdentityMenuAppService
    : ICrudAppService<
        IdentityMenuDto,
        Guid,
        GetIdentityMenusInput,
        IdentityMenuDto,
        IdentityMenuCreateDto,
        IdentityMenuUpdateDto>
{
    Task MultiCreateAsync(List<IdentityMenuCreateDto> inputs);

    Task<PagedResultDto<IdentityMenuLiteDto>> GetLiteListAsync(GetIdentityMenusInput input);
    Task<ListResultDto<IdentityMenuTreeDto>> GetTreeAsync();

    Task<ListResultDto<TreeSelectGuidDto>> GetTreeSelectAsync();
}