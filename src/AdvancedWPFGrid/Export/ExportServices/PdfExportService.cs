using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedWPFGrid.Columns;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace AdvancedWPFGrid.Export.ExportServices;

/// <summary>
/// Exports grid data to PDF using PDFSharp/MigraDoc (MIT Licensed).
/// </summary>
public class PdfExportService : IExportService
{
    public string FormatName => "PDF";
    public string FileExtension => ".pdf";
    public string FileFilter => "PDF Document (*.pdf)|*.pdf";

    public async Task ExportAsync(IEnumerable data, IEnumerable<GridExportColumn> columns, ExportOptions options, IProgress<double>? progress = null)
    {
        if (string.IsNullOrEmpty(options.FilePath))
            throw new System.ArgumentException("File path must be specified.", nameof(options));

        await Task.Run(() =>
        {
            var document = new Document();
            document.Info.Title = options.Title;

            // Styles
            Style style = document.Styles["Normal"];
            style.Font.Name = "Verdana";
            style.Font.Size = 8;

            Section section = document.AddSection();
            section.PageSetup.Orientation = Orientation.Landscape;
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.LeftMargin = "1cm";
            section.PageSetup.RightMargin = "1cm";

            // Title
            Paragraph title = section.AddParagraph(options.Title);
            title.Format.Font.Size = 14;
            title.Format.Font.Bold = true;
            title.Format.SpaceAfter = "0.5cm";
            title.Format.Font.Color = Colors.DarkBlue;

            // Table
            Table table = section.AddTable();
            table.Borders.Width = 0.75;
            table.Borders.Color = Colors.LightGray;

            var columnList = columns.ToList();
            var itemList = data.Cast<object>().ToList();
            int totalRows = itemList.Count;

            // Define Columns
            foreach (var col in columnList)
            {
                table.AddColumn(); // Auto-width isn't great in MigraDoc, but we can't easily measure without a graphics context here.
            }

            // Header Row
            if (options.IncludeHeaders)
            {
                Row headerRow = table.AddRow();
                headerRow.HeadingFormat = true;
                headerRow.Format.Font.Bold = true;
                headerRow.Shading.Color = Colors.AliceBlue;

                for (int i = 0; i < columnList.Count; i++)
                {
                    headerRow.Cells[i].AddParagraph(columnList[i].Header);
                    headerRow.Cells[i].VerticalAlignment = VerticalAlignment.Center;
                }
            }

            // Data Rows
            for (int i = 0; i < totalRows; i++)
            {
                var item = itemList[i];
                Row row = table.AddRow();
                row.VerticalAlignment = VerticalAlignment.Center;

                for (int j = 0; j < columnList.Count; j++)
                {
                    var value = columnList[j].GetValue(item, true)?.ToString() ?? string.Empty;
                    row.Cells[j].AddParagraph(value);
                }

                // Report progress
                progress?.Report((double)i / totalRows * 80.0); // Reserve 20% for rendering
            }

            // Render and Save
            progress?.Report(85.0);
            PdfDocumentRenderer renderer = new PdfDocumentRenderer();
            renderer.Document = document;
            renderer.RenderDocument();
            progress?.Report(95.0);
            renderer.PdfDocument.Save(options.FilePath);
            progress?.Report(100.0);
        });
    }
}
