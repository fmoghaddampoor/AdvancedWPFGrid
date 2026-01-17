using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace EasyUse
{
    public class MainViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public ObservableCollection<PersonModel> People { get; set; }
        public List<string> Roles { get; set; }

        private int _salaryDecimalPlaces = 5;
        public int SalaryDecimalPlaces
        {
            get => _salaryDecimalPlaces;
            set
            {
                if (_salaryDecimalPlaces != value)
                {
                    _salaryDecimalPlaces = value;
                    OnPropertyChanged(nameof(SalaryDecimalPlaces));
                    OnPropertyChanged(nameof(SalaryFormatString));
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SalaryFormatString
        {
            get
            {
                if (SalaryDecimalPlaces <= 0)
                    return "'$'0";

                return "'$'0." + new string('0', SalaryDecimalPlaces);
            }
        }

        public System.Windows.Input.ICommand IncrementDecimalsCommand { get; }
        public System.Windows.Input.ICommand DecrementDecimalsCommand { get; }

        public MainViewModel(AppData appData)
        {
            Roles = appData.Roles ?? new List<string>();
            People = new ObservableCollection<PersonModel>(appData.People ?? new List<PersonModel>());

            IncrementDecimalsCommand = new RelayCommand(_ => SalaryDecimalPlaces++, _ => SalaryDecimalPlaces < 5);
            DecrementDecimalsCommand = new RelayCommand(_ => SalaryDecimalPlaces--, _ => SalaryDecimalPlaces > 0);
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => 
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
