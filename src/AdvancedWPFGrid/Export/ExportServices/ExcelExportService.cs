using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AdvancedWPFGrid.Columns;
using ClosedXML.Excel;

namespace AdvancedWPFGrid.Export.ExportServices;

/// <summary>
/// Exports grid data to Excel (.xlsx) format using ClosedXML.
/// </summary>
public class ExcelExportService : IExportService
{
    public string FormatName => "Excel";
    public string FileExtension => ".xlsx";
    public string FileFilter => "Excel Workbook (*.xlsx)|*.xlsx";

    public async Task ExportAsync(IEnumerable data, IEnumerable<GridExportColumn> columns, ExportOptions options)
    {
        if (string.IsNullOrEmpty(options.FilePath))
            throw new System.ArgumentException("File path must be specified.", nameof(options));

        await Task.Run(() =>
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(options.Title);

            int currentRow = 1;
            int currentColumn = 1;

            var columnList = columns.ToList();

            // Write Headers
            if (options.IncludeHeaders)
            {
                foreach (var col in columnList)
                {
                    var cell = worksheet.Cell(currentRow, currentColumn);
                    cell.Value = col.Header;
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    currentColumn++;
                }
                currentRow++;
            }

            // Write Data
            foreach (var item in data)
            {
                currentColumn = 1;
                foreach (var col in columnList)
                {
                    var value = col.GetValue(item);
                    var cell = worksheet.Cell(currentRow, currentColumn);
                    
                    if (value != null)
                    {
                        cell.Value = XLCellValue.FromObject(value);
                    }
                    
                    currentColumn++;
                }
                currentRow++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(options.FilePath);
        });
    }
}
