using Cits;
using Cits.Dtos;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface IOrganizationUnitService : ICrudAppService<
    OrganizationUnitDto,
    Guid,
    GetOrganizationUnitInput,
    OrganizationUnitDto,
    OrganizationUnitCreateUpdateDto,
    OrganizationUnitCreateUpdateDto>
{
    Task<ListResultDto<OrganizationUnitTreeDto>> GetTreeAsync();
}