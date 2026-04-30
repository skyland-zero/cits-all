using System.Text;
using MyApi.Application.Exports.Dto;

namespace MyApi.Application.Exports;

public static class CsvExportFileBuilder
{
    public static byte[] Build(
        IReadOnlyList<ExportFieldDto> fields,
        IReadOnlyList<string> selectedKeys,
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        var fieldMap = fields.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        var selectedFields = selectedKeys
            .Where(fieldMap.ContainsKey)
            .Select(x => fieldMap[x])
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", selectedFields.Select(x => Escape(x.Label))));

        foreach (var row in rows)
        {
            var values = selectedFields.Select(field =>
                row.TryGetValue(field.Key, out var value) ? Escape(FormatValue(value)) : string.Empty);
            builder.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            DateTimeOffset dateTimeOffset => dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss"),
            bool boolean => boolean ? "是" : "否",
            _ => Convert.ToString(value) ?? string.Empty
        };
    }

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
