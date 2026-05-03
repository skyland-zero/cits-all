using ClosedXML.Excel;

namespace MyApi.Application.Imports;

public sealed class XlsxImportRow
{
    public int RowNumber { get; set; }

    public Dictionary<string, string?> Values { get; set; } = [];
}

public static class XlsxImportReader
{
    public static List<XlsxImportRow> ReadRows(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();
        var headerRow = sheet.Row(2);
        var lastColumn = sheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 0;
        if (lastColumn == 0 || lastRow <= 3) return [];

        var keys = Enumerable.Range(1, lastColumn)
            .Select(index => headerRow.Cell(index).GetString().Trim())
            .ToList();

        var rows = new List<XlsxImportRow>();
        for (var rowNumber = 4; rowNumber <= lastRow; rowNumber++)
        {
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            var hasValue = false;
            for (var col = 1; col <= lastColumn; col++)
            {
                var key = keys[col - 1];
                if (string.IsNullOrWhiteSpace(key)) continue;

                var value = sheet.Cell(rowNumber, col).GetString().Trim();
                if (!string.IsNullOrWhiteSpace(value)) hasValue = true;
                values[key] = value;
            }

            if (hasValue)
            {
                rows.Add(new XlsxImportRow { RowNumber = rowNumber, Values = values });
            }
        }

        return rows;
    }
}
