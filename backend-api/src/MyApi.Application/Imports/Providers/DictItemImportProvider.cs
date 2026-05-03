using Cits.Domain.Dictionaries;
using Cits.IdGenerator;
using MyApi.Application.Imports.Dto;

namespace MyApi.Application.Imports.Providers;

public class DictItemImportProvider : IImportProvider
{
    private readonly IFreeSql _freeSql;
    private readonly IIdGenerator _idGenerator;

    public DictItemImportProvider(IFreeSql freeSql, IIdGenerator idGenerator)
    {
        _freeSql = freeSql;
        _idGenerator = idGenerator;
    }

    public string ModuleKey => "system.dict-items";

    public string ModuleName => "字典数据";

    public IReadOnlyList<ImportTemplateColumnDto> Columns =>
    [
        new() { Key = "DictCode", Label = "字典编码", Required = true, Description = "已存在的字典分类编码，如 sys_user_status" },
        new() { Key = "Label", Label = "显示标签", Required = true, Description = "字典项显示文本，如 启用" },
        new() { Key = "Value", Label = "数据值", Required = true, Description = "字典项值，同一字典编码下唯一，如 1" },
        new() { Key = "Sort", Label = "排序", Required = false, Description = "整数，默认 0" },
        new() { Key = "IsEnabled", Label = "启用", Required = false, Description = "true/false、是/否、1/0，默认 true" }
    ];

    public async Task<ImportProviderResult> ImportAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var rows = XlsxImportReader.ReadRows(stream);
        var result = new ImportProviderResult { TotalCount = rows.Count };
        if (rows.Count == 0) return result;

        var dictCodes = rows
            .Select(x => GetValue(x, "DictCode"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var dictTypes = await _freeSql.Select<DataDictType>()
            .Where(x => dictCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);
        var dictTypeMap = dictTypes.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            try
            {
                var dictCode = Require(row, "DictCode", "字典编码不能为空");
                var label = Require(row, "Label", "显示标签不能为空");
                var value = Require(row, "Value", "数据值不能为空");
                var sort = ParseInt(GetValue(row, "Sort"), 0);
                var isEnabled = ParseBool(GetValue(row, "IsEnabled"), true);

                if (!dictTypeMap.TryGetValue(dictCode, out var dictType))
                {
                    throw new InvalidOperationException($"字典编码 {dictCode} 不存在");
                }

                var existing = await _freeSql.Select<DataDictItem>()
                    .Where(x => x.DictTypeId == dictType.Id && x.Value == value)
                    .FirstAsync(cancellationToken);

                if (existing == null)
                {
                    var entity = new DataDictItem
                    {
                        Id = _idGenerator.Create(),
                        DictTypeId = dictType.Id,
                        Label = label,
                        Value = value,
                        Sort = sort,
                        IsEnabled = isEnabled
                    };
                    await _freeSql.Insert(entity).ExecuteAffrowsAsync(cancellationToken);
                }
                else
                {
                    existing.Label = label;
                    existing.Sort = sort;
                    existing.IsEnabled = isEnabled;
                    await _freeSql.Update<DataDictItem>().SetSource(existing).ExecuteAffrowsAsync(cancellationToken);
                }

                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new ImportRowErrorDto
                {
                    RowNumber = row.RowNumber,
                    ErrorMessage = ex.Message,
                    Values = row.Values
                });
            }
        }

        return result;
    }

    private static string Require(XlsxImportRow row, string key, string errorMessage)
    {
        var value = GetValue(row, key);
        if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(errorMessage);
        return value;
    }

    private static string GetValue(XlsxImportRow row, string key)
    {
        return row.Values.TryGetValue(key, out var value) ? value?.Trim() ?? string.Empty : string.Empty;
    }

    private static int ParseInt(string? value, int defaultValue)
    {
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (bool.TryParse(value, out var boolValue)) return boolValue;

        return value.Trim() switch
        {
            "1" => true,
            "0" => false,
            "是" => true,
            "否" => false,
            "启用" => true,
            "禁用" => false,
            _ => defaultValue
        };
    }
}
