using ClosedXML.Excel;
using MyApi.Application.Imports.Dto;

namespace MyApi.Application.Imports;

public static class XlsxImportTemplateBuilder
{
    public static byte[] Build(IReadOnlyList<ImportTemplateColumnDto> columns)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("导入模板");

        for (var i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            var cell = sheet.Cell(1, i + 1);
            cell.Value = column.Required ? $"{column.Label}*" : column.Label;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F3F4F6");
            sheet.Cell(2, i + 1).Value = column.Key;
            sheet.Cell(3, i + 1).Value = column.Description ?? string.Empty;
        }

        sheet.Row(2).Hide();
        sheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
