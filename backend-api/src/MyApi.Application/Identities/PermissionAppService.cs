using Cits.Dtos;
using Cits.Permissions;
using Microsoft.AspNetCore.Authorization;
using MyApi.Application.Identities.Dto;

namespace MyApi.Application.Identities;

[Authorize]
public class PermissionAppService : IPermissionAppService
{
    private readonly IFreeSql _freeSql;

    public PermissionAppService(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public async Task<ListResultDto<TreeSelectStringDto>> GetTreeSelectAsync()
    {
        var groups = await _freeSql.Select<PermissionGroup>()
            .Where(x => x.IsEnabled == true)
            .ToListAsync();
        var permissions = await _freeSql.Select<Permission>()
            .Where(x => x.IsEnabled == true)
            .ToListAsync();

        var list = new List<TreeSelectStringDto>();
        foreach (var group in groups)
        {
            var tempPermissions = permissions
                .Where(x => x.GroupCode == group.Code)
                .Select(x => new TreeSelectStringDto
                {
                    Value = x.Code,
                    Label = x.DisplayName,
                    ParentId = x.GroupCode
                }).ToList();
            var temp = new TreeSelectStringDto
            {
                Value = group.Code,
                Label = group.DisplayName,
                Children = tempPermissions
            };
            list.Add(temp);
        }

        return new ListResultDto<TreeSelectStringDto>(list);
    }
}