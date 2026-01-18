using System.Windows;

namespace AdvancedWPFGrid.Export.UI;

public partial class ExportSettingsWindow : Window
{
    public ExportSettingsWindow(ExportSettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.CloseRequested += (s, e) => 
        {
            if (viewModel.DialogResult.HasValue)
            {
                try { this.DialogResult = viewModel.DialogResult; } catch { }
            }
            this.Close();
        };

        this.Loaded += (s, e) =>
        {
            if (this.Owner != null)
            {
                this.Icon = this.Owner.Icon;
            }
        };
    }
}
