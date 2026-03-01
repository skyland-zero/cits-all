using Cits.DI;
using Cits.Dtos;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface IPermissionAppService : IApplicationService
{
    Task<ListResultDto<TreeSelectStringDto>> GetTreeSelectAsync();
}