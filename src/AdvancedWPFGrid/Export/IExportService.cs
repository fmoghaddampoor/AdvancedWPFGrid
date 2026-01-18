using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvancedWPFGrid.Export;

/// <summary>
/// A thread-safe model for a grid column used in exporting.
/// </summary>
public class GridExportColumn : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isSelected = true;
    public string Header { get; set; } = string.Empty;
    public string? Binding { get; set; }
    public string? StringFormat { get; set; }
    
    public bool IsSelected 
    { 
        get => _isSelected; 
        set { _isSelected = value; OnPropertyChanged(); } 
    }
    
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

    // Helper to get value via reflection (thread-safe if target object is a POCO)
    public object? GetValue(object item, bool formatted = false)
    {
        if (string.IsNullOrEmpty(Binding)) return null;

        try
        {
            var prop = item.GetType().GetProperty(Binding);
            var value = prop?.GetValue(item);

            if (formatted && value != null && !string.IsNullOrEmpty(StringFormat))
            {
                return string.Format(StringFormat, value);
            }

            return value;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Options for configuring data export.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Gets or sets the title of the exported document.
    /// </summary>
    public string Title { get; set; } = "Exported Data";

    /// <summary>
    /// Gets or sets whether to include column headers in the export.
    /// </summary>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to only export rows that are visible in the grid (respecting filters).
    /// </summary>
    public bool VisibleRowsOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to include the summary row in the export.
    /// </summary>
    public bool IncludeSummary { get; set; } = false;

    /// <summary>
    /// Gets or sets the file path to save the export to.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets whether to automatically open the file after export.
    /// </summary>
    public bool OpenAfterExport { get; set; } = true;
}

/// <summary>
/// Defines a service capable of exporting grid data to a specific format.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Gets the display name of the export format (e.g. "Excel").
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// Gets the file extension for this format (e.g. ".xlsx").
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets a filter string for the SaveFileDialog.
    /// </summary>
    string FileFilter { get; }

    /// <summary>
    /// Exports the provided data to the specified location.
    /// </summary>
    /// <param name="data">The list of items to export.</param>
    /// <param name="columns">The columns metadata extracted from the UI thread.</param>
    /// <param name="options">Configuration options for the export.</param>
    /// <param name="progress">Progress reporter (0.0 to 100.0).</param>
    Task ExportAsync(IEnumerable data, IEnumerable<GridExportColumn> columns, ExportOptions options, IProgress<double>? progress = null);
}
