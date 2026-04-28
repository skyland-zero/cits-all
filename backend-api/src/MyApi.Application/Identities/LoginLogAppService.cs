using Cits.Dtos;
using Cits.LoginLogs;
using Mapster;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

public class LoginLogAppService : ILoginLogAppService
{
    private readonly IFreeSql _freeSql;

    public LoginLogAppService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public async Task<PagedResultDto<LoginLogDto>> GetListAsync(GetLoginLogsInput input)
    {
        var list = await _freeSql.Select<LoginLog>()
            .WhereIf(!string.IsNullOrWhiteSpace(input.UserName), a => a.UserName.Contains(input.UserName!) || a.RealName.Contains(input.UserName!))
            .WhereIf(!string.IsNullOrWhiteSpace(input.IP), a => a.IP.Contains(input.IP!))
            .WhereIf(input.Status.HasValue, a => a.Status == input.Status!.Value)
            .OrderByDescending(a => a.LoginTime)
            .Count(out var totalCount)
            .Page(input.SkipCount / input.MaxResultCount + 1, input.MaxResultCount)
            .ToListAsync();

        return new PagedResultDto<LoginLogDto>(totalCount, list.Adapt<List<LoginLogDto>>());
    }
}