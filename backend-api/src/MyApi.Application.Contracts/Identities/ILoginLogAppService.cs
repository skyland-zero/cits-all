using Cits.Dtos;
using Cits.DI;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public interface ILoginLogAppService : IApplicationService
{
    Task<PagedResultDto<LoginLogDto>> GetListAsync(GetLoginLogsInput input);
}