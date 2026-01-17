using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace EasyUse
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public ObservableCollection<PersonModel> People { get; set; }
        public List<string> Roles { get; set; }
        
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
