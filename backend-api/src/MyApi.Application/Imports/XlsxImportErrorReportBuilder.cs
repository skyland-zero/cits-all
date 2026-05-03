using ClosedXML.Excel;
using MyApi.Application.Imports.Dto;

namespace MyApi.Application.Imports;

public static class XlsxImportErrorReportBuilder
{
    public static byte[] Build(IReadOnlyList<ImportTemplateColumnDto> columns, IReadOnlyList<ImportRowErrorDto> errors)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("错误报告");
        sheet.Cell(1, 1).Value = "行号";
        sheet.Cell(1, 2).Value = "错误信息";

        for (var i = 0; i < columns.Count; i++)
        {
            sheet.Cell(1, i + 3).Value = columns[i].Label;
        }

        var rowIndex = 2;
        foreach (var error in errors)
        {
            sheet.Cell(rowIndex, 1).Value = error.RowNumber;
            sheet.Cell(rowIndex, 2).Value = error.ErrorMessage;
            for (var i = 0; i < columns.Count; i++)
            {
                error.Values.TryGetValue(columns[i].Key, out var value);
                sheet.Cell(rowIndex, i + 3).Value = value ?? string.Empty;
            }
            rowIndex++;
        }

        sheet.Row(1).Style.Font.Bold = true;
        sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
