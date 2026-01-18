using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AdvancedWPFGrid.Common;

namespace AdvancedWPFGrid.Export.UI;

public class ExportSettingsViewModel : INotifyPropertyChanged
{
    private ExportOptions _options;
    private bool? _allColumnsSelected = true;

    public ExportSettingsViewModel(ExportOptions options, IEnumerable<GridExportColumn> columns)
    {
        _options = options;
        Columns = new ObservableCollection<GridExportColumn>(columns);
        
        foreach (var col in Columns)
        {
            if (col is INotifyPropertyChanged npc)
            {
                // Note: GridExportColumn needs to support PropertyChanged if we want reactive "Select All"
                // For now we'll do manual toggle.
            }
        }

        OkCommand = new RelayCommand(_ => { DialogResult = true; CloseRequested?.Invoke(this, EventArgs.Empty); });
        CancelCommand = new RelayCommand(_ => { DialogResult = false; CloseRequested?.Invoke(this, EventArgs.Empty); });
        ToggleAllCommand = new RelayCommand(_ => ToggleAll());
    }

    public ExportOptions Options => _options;
    public ObservableCollection<GridExportColumn> Columns { get; }
    
    public bool? AllColumnsSelected
    {
        get => _allColumnsSelected;
        set { _allColumnsSelected = value; OnPropertyChanged(); }
    }

    public bool? DialogResult { get; private set; }
    public event EventHandler? CloseRequested;

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ToggleAllCommand { get; }

    private void ToggleAll()
    {
        bool target = AllColumnsSelected != true;
        foreach (var col in Columns)
        {
            col.IsSelected = target;
        }
        AllColumnsSelected = target;
        // Refresh UI if necessary (relies on GridExportColumn being observable)
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
