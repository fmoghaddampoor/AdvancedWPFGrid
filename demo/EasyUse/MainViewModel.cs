using System.Collections.ObjectModel;
using System.Collections.Generic;

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
                    OnPropertyChanged(nameof(SelectedGroup1));
                    UpdateGroupDescriptors();
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
                    OnPropertyChanged(nameof(SelectedGroup2));
                    UpdateGroupDescriptors();
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
                    OnPropertyChanged(nameof(SelectedGroup3));
                    UpdateGroupDescriptors();
                }
            }
        }

        public ObservableCollection<System.ComponentModel.GroupDescription> GroupDescriptors { get; } = new();

        private void UpdateGroupDescriptors()
        {
            GroupDescriptors.Clear();

            AddGroupDescriptor(SelectedGroup1);
            AddGroupDescriptor(SelectedGroup2);
            AddGroupDescriptor(SelectedGroup3);
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
                    OnPropertyChanged(nameof(DecimalPlaces));
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
                    OnPropertyChanged(nameof(GlobalDoubleFormat));
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
                    OnPropertyChanged(nameof(HasVerticalGridLines));
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
                    OnPropertyChanged(nameof(HasHorizontalGridLines));
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
                    OnPropertyChanged(nameof(AlternatingRows));
                }
            }
        }

        public System.Windows.Input.ICommand IncrementDecimalsCommand { get; }
        public System.Windows.Input.ICommand DecrementDecimalsCommand { get; }

        public MainViewModel(AppData appData)
        {
            Roles = appData.Roles ?? new List<string>();
            People = new ObservableCollection<PersonModel>(appData.People ?? new List<PersonModel>());
            
            // Initial format update
            GlobalDoubleFormat = $"F{DecimalPlaces}";

            IncrementDecimalsCommand = new RelayCommand(_ => DecimalPlaces++, _ => DecimalPlaces < 5);
            DecrementDecimalsCommand = new RelayCommand(_ => DecimalPlaces--, _ => DecimalPlaces > 0);
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
