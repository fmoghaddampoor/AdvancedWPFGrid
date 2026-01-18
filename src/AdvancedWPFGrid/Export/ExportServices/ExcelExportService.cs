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

    public async Task ExportAsync(IEnumerable data, IEnumerable<GridExportColumn> columns, ExportOptions options, IProgress<double>? progress = null)
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
            var itemList = data.Cast<object>().ToList();
            int totalRows = itemList.Count;

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
            for (int i = 0; i < totalRows; i++)
            {
                var item = itemList[i];
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

                // Report progress
                progress?.Report((double)i / totalRows * 90.0); // Reserve 10% for saving
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();
            progress?.Report(95.0);

            workbook.SaveAs(options.FilePath);
            progress?.Report(100.0);
        });
    }
}
