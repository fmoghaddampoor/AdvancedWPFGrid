using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvancedWPFGrid.Columns;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AdvancedWPFGrid.Export.ExportServices;

/// <summary>
/// Exports grid data to PDF format using QuestPDF.
/// </summary>
public class PdfExportService : IExportService
{
    public PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public string FormatName => "PDF";
    public string FileExtension => ".pdf";
    public string FileFilter => "PDF Document (*.pdf)|*.pdf";

    public async Task ExportAsync(IEnumerable data, IEnumerable<GridExportColumn> columns, ExportOptions options)
    {
        if (string.IsNullOrEmpty(options.FilePath))
            throw new System.ArgumentException("File path must be specified.", nameof(options));

        // Create the document
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                page.Header().Text(options.Title)
                    .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    var visibleColumns = columns.ToList();
                    
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var col in visibleColumns)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    // Header
                    if (options.IncludeHeaders)
                    {
                        table.Header(header =>
                        {
                            foreach (var col in visibleColumns)
                            {
                                header.Cell().Element(CellStyle).Text(col.Header).SemiBold();
                            }

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.DefaultTextStyle(x => x.SemiBold())
                                                .PaddingVertical(5)
                                                .BorderBottom(1)
                                                .BorderColor(Colors.Black);
                            }
                        });
                    }

                    // Content
                    foreach (var item in data)
                    {
                        foreach (var col in visibleColumns)
                        {
                            var value = col.GetValue(item)?.ToString() ?? string.Empty;
                            table.Cell().Element(CellStyle).Text(value);
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(5);
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        await Task.Run(() => document.GeneratePdf(options.FilePath));
    }
}
