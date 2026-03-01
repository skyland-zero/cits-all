using Cits;
using Cits.Dtos;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface IIdentityPageAppService
    : ICrudAppService<
        IdentityPageDto,
        Guid,
        GetIdentityPagesInput,
        IdentityPageDto,
        IdentityPageCreateUpdateDto,
        IdentityPageCreateUpdateDto>
{
    Task<ListResultDto<IdentityPageTreeDto>> GetTreeAsync();
    Task<ListResultDto<TreeSelectGuidDto>> GetTreeSelectAsync();
}