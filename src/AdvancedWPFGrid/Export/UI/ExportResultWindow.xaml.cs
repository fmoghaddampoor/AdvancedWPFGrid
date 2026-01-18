using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AdvancedWPFGrid.Export.UI;

public partial class ExportResultWindow : Window
{
    private readonly string _filePath;

    public ExportResultWindow(string filePath)
    {
        InitializeComponent();
        _filePath = filePath;
        this.Loaded += (s, e) =>
        {
            if (this.Owner != null)
            {
                this.Icon = this.Owner.Icon;
            }
        };
    }

    private void OpenFile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(_filePath) { UseShellExecute = true });
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowInFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("explorer.exe", $"/select,\"{_filePath}\"");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
