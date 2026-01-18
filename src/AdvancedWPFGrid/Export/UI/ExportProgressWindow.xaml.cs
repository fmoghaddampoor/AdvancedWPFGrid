using System;
using System.Windows;
using System.Windows.Controls;

namespace AdvancedWPFGrid.Export.UI;

public partial class ExportProgressWindow : Window, IProgress<double>
{
    public ExportProgressWindow()
    {
        InitializeComponent();
        this.Loaded += (s, e) =>
        {
            if (this.Owner != null)
            {
                this.Icon = this.Owner.Icon;
            }
        };
    }

    public void Report(double value)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            ExportProgressBar.Value = value;
            ProgressText.Text = $"{(int)value}%";
        }));
    }
}
