#pragma warning disable SA1101, SA1309 // Prefix local calls with this, Field should not begin with underscore
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using AdvancedWPFGrid.Controls;
using AdvancedWPFGrid.Managers;

namespace EasyUse
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public ObservableCollection<PersonModel> People { get; set; }
        public List<string> Roles { get; set; }
        

        public List<string> AvailableColumns { get; } = new List<string> 
        { 
            "(None)", 
            "Name", 
            "Age", 
            "Role", 
            "IsActive", 
            "Salary", 
            "PerformanceScore" 
        };

        private string? _selectedGroup1 = "(None)";
        public string? SelectedGroup1
        {
            get => _selectedGroup1;
            set
            {
                if (_selectedGroup1 != value)
                {
                    _selectedGroup1 = value;
                    this.OnPropertyChanged(nameof(SelectedGroup1));
                    this.UpdateGroupDescriptors();
                }
            }
        }

        private string? _selectedGroup2 = "(None)";
        public string? SelectedGroup2
        {
            get => _selectedGroup2;
            set
            {
                if (_selectedGroup2 != value)
                {
                    _selectedGroup2 = value;
                    this.OnPropertyChanged(nameof(SelectedGroup2));
                    this.UpdateGroupDescriptors();
                }
            }
        }

        private string? _selectedGroup3 = "(None)";
        public string? SelectedGroup3
        {
            get => _selectedGroup3;
            set
            {
                if (_selectedGroup3 != value)
                {
                    _selectedGroup3 = value;
                    this.OnPropertyChanged(nameof(SelectedGroup3));
                    this.UpdateGroupDescriptors();
                }
            }
        }

        public ObservableCollection<System.ComponentModel.GroupDescription> GroupDescriptors { get; } = new();

        private void UpdateGroupDescriptors()
        {
            GroupDescriptors.Clear();

            this.AddGroupDescriptor(SelectedGroup1);
            this.AddGroupDescriptor(SelectedGroup2);
            this.AddGroupDescriptor(SelectedGroup3);
        }

        private void AddGroupDescriptor(string? propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName != "(None)")
            {
                GroupDescriptors.Add(new System.Windows.Data.PropertyGroupDescription(propertyName));
            }
        }
        
        private int _decimalPlaces = 2;
        public int DecimalPlaces
        {
            get => _decimalPlaces;
            set
            {
                if (_decimalPlaces != value)
                {
                    _decimalPlaces = value;
                    GlobalDoubleFormat = $"F{value}";
                    this.OnPropertyChanged(nameof(DecimalPlaces));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }


        private string _globalDoubleFormat = "P2";
        public string GlobalDoubleFormat
        {
            get => _globalDoubleFormat;
            set
            {
                if (_globalDoubleFormat != value)
                {
                    _globalDoubleFormat = value;
                    this.OnPropertyChanged(nameof(GlobalDoubleFormat));
                }
            }
        }

        private bool _hasVerticalGridLines = true;
        public bool HasVerticalGridLines
        {
            get => _hasVerticalGridLines;
            set
            {
                if (_hasVerticalGridLines != value)
                {
                    _hasVerticalGridLines = value;
                    this.OnPropertyChanged(nameof(HasVerticalGridLines));
                }
            }
        }

        private bool _hasHorizontalGridLines = true;
        public bool HasHorizontalGridLines
        {
            get => _hasHorizontalGridLines;
            set
            {
                if (_hasHorizontalGridLines != value)
                {
                    _hasHorizontalGridLines = value;
                    this.OnPropertyChanged(nameof(HasHorizontalGridLines));
                }
            }
        }

        private bool _alternatingRows = true;
        public bool AlternatingRows
        {
            get => _alternatingRows;
            set
            {
                if (_alternatingRows != value)
                {
                    _alternatingRows = value;
                    this.OnPropertyChanged(nameof(AlternatingRows));
                }
            }
        }

        private bool _canUserSort = true;
        public bool CanUserSort
        {
            get => _canUserSort;
            set
            {
                if (_canUserSort != value)
                {
                    _canUserSort = value;
                    this.OnPropertyChanged(nameof(CanUserSort));
                }
            }
        }

        private bool _canUserFilter = true;
        public bool CanUserFilter
        {
            get => _canUserFilter;
            set
            {
                if (_canUserFilter != value)
                {
                    _canUserFilter = value;
                    this.OnPropertyChanged(nameof(CanUserFilter));
                }
            }
        }

        private bool _isSearchPanelVisible = false;
        public bool IsSearchPanelVisible
        {
            get => _isSearchPanelVisible;
            set
            {
                if (_isSearchPanelVisible != value)
                {
                    _isSearchPanelVisible = value;
                    this.OnPropertyChanged(nameof(IsSearchPanelVisible));
                }
            }
        }

        private bool _isAutoSearchEnabled = true;
        public bool IsAutoSearchEnabled
        {
            get => _isAutoSearchEnabled;
            set
            {
                if (_isAutoSearchEnabled != value)
                {
                    _isAutoSearchEnabled = value;
                    this.OnPropertyChanged(nameof(IsAutoSearchEnabled));
                }
            }
        }

        private SearchCountMode _searchCountMode = SearchCountMode.Rows;
        public SearchCountMode SearchCountMode
        {
            get => _searchCountMode;
            set
            {
                if (_searchCountMode != value)
                {
                    _searchCountMode = value;
                    this.OnPropertyChanged(nameof(SearchCountMode));
                }
            }
        }

        private double _searchDelaySeconds = 0.5;
        public double SearchDelaySeconds
        {
            get => _searchDelaySeconds;
            set
            {
                if (Math.Abs(_searchDelaySeconds - value) > 0.001)
                {
                    _searchDelaySeconds = value;
                    this.OnPropertyChanged(nameof(SearchDelaySeconds));
                    this.OnPropertyChanged(nameof(SearchDelay));
                }
            }
        }

        public TimeSpan SearchDelay => TimeSpan.FromSeconds(_searchDelaySeconds);

        private bool _canUserReorderColumns = true;
        public bool CanUserReorderColumns
        {
            get => _canUserReorderColumns;
            set
            {
                if (_canUserReorderColumns != value)
                {
                    _canUserReorderColumns = value;
                    this.OnPropertyChanged(nameof(CanUserReorderColumns));
                }
            }
        }

        private bool _canUserResizeColumns = true;
        public bool CanUserResizeColumns
        {
            get => _canUserResizeColumns;
            set
            {
                if (_canUserResizeColumns != value)
                {
                    _canUserResizeColumns = value;
                    this.OnPropertyChanged(nameof(CanUserResizeColumns));
                }
            }
        }

        public System.Windows.Input.ICommand IncrementDecimalsCommand { get; }
        public System.Windows.Input.ICommand DecrementDecimalsCommand { get; }

        public MainViewModel(AppData appData)
        {
            this.Roles = appData.Roles ?? new List<string>();
            this.People = new ObservableCollection<PersonModel>(appData.People ?? new List<PersonModel>());
            
            // Initial format update
            this.GlobalDoubleFormat = $"F{this.DecimalPlaces}";

            this.IncrementDecimalsCommand = new RelayCommand(_ => this.DecimalPlaces++, _ => this.DecimalPlaces < 5);
            this.DecrementDecimalsCommand = new RelayCommand(_ => this.DecimalPlaces--, _ => this.DecimalPlaces > 0);
        }

        public IEnumerable<SearchCountMode> SearchCountModes => Enum.GetValues(typeof(SearchCountMode)).Cast<SearchCountMode>();

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => 
            this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
