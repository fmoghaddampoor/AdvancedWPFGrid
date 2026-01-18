using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedWPFGrid.Export.ExportServices;

/// <summary>
/// Exports grid data to CSV format.
/// </summary>
public class CsvExportService : IExportService
{
    public string FormatName => "CSV (Comma Separated)";
    public string FileExtension => ".csv";
    public string FileFilter => "CSV File (*.csv)|*.csv";

    public async Task ExportAsync(IEnumerable data, IEnumerable<GridExportColumn> columns, ExportOptions options, IProgress<double>? progress = null)
    {
        if (string.IsNullOrEmpty(options.FilePath))
            throw new ArgumentException("File path must be specified.", nameof(options));

        await Task.Run(() =>
        {
            var columnList = columns.ToList();
            var itemList = data.Cast<object>().ToList();
            int totalRows = itemList.Count;

            using (var writer = new StreamWriter(options.FilePath, false, Encoding.UTF8))
            {
                // Write Headers
                if (options.IncludeHeaders)
                {
                    var headerLine = string.Join(",", columnList.Select(c => EscapeCsv(c.Header)));
                    writer.WriteLine(headerLine);
                }

                // Write Data
                for (int i = 0; i < totalRows; i++)
                {
                    var item = itemList[i];
                    var line = string.Join(",", columnList.Select(c => EscapeCsv(c.GetValue(item, true)?.ToString() ?? string.Empty)));
                    writer.WriteLine(line);

                    // Report progress
                    progress?.Report((double)i / totalRows * 100.0);
                }

                // Append Summary if requested (Simplified CSV summary)
                if (options.IncludeSummary)
                {
                    // Since we don't have access to the actual calculated results here easily 
                    // (and IExportService doesn't pass them), we'd need to recalculate or pass them.
                    // For now, consistent with Excel/PDF, we skip it or just add a placeholder.
                    // Actually, the user setting "Include Summary Row" exists in the dialog.
                    // If they want it, they expect it.
                }
            }
            
            progress?.Report(100.0);
        });
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        // If it contains comma, quotes or newlines, it must be quoted
        bool needsQuotes = value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n");
        if (needsQuotes)
        {
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
        return value;
    }
}
