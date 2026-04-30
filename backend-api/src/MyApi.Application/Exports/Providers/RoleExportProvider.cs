using System.Text.Json;
using Cits.Extensions;
using MyApi.Application.Exports.Dto;
using MyApi.Application.Identities.Dto;
using MyApi.Domain.Identities;

namespace MyApi.Application.Exports.Providers;

public class RoleExportProvider : IExportProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IFreeSql _freeSql;

    public RoleExportProvider(IFreeSql freeSql)
    {
        _freeSql = freeSql;
    }

    public string ModuleKey => "basic.role";

    public string ModuleName => "角色";

    public IReadOnlyList<ExportFieldDto> Fields { get; } =
    [
        new("name", "角色名称"),
        new("code", "角色编码"),
        new("description", "说明"),
        new("isDefault", "默认角色", false),
        new("isStatic", "静态角色", false)
    ];

    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> GetRowsAsync(
        string queryJson,
        CancellationToken cancellationToken = default)
    {
        var input = JsonSerializer.Deserialize<GetRolesInput>(queryJson, JsonOptions) ?? new GetRolesInput();
        var name = input.Name ?? string.Empty;
        var code = input.Code ?? string.Empty;

        var list = await _freeSql.Select<IdentityRole>()
            .WhereIf(!name.IsNullOrWhiteSpace(), x => x.Name.Contains(name))
            .WhereIf(!code.IsNullOrWhiteSpace(), x => x.Code == code)
            .WhereIf(input.IsDefault.HasValue, x => x.IsDefault == input.IsDefault)
            .WhereIf(input.IsStatic.HasValue, x => x.IsStatic == input.IsStatic)
            .OrderByDescending(x => x.CreationTime)
            .ToListAsync(cancellationToken);

        return list.Select(x => (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?>
        {
            ["name"] = x.Name,
            ["code"] = x.Code,
            ["description"] = x.Description,
            ["isDefault"] = x.IsDefault,
            ["isStatic"] = x.IsStatic
        }).ToList();
    }
}
