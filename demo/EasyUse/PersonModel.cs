using System;
using System.Windows.Input;
using System.Windows;

namespace EasyUse
{
    public class PersonModel
    {
        public string Name { get; set; } = string.Empty;
        public double Age { get; set; }
        public bool IsActive { get; set; }
        public string Role { get; set; } = "User";
        public double Salary { get; set; }
        
        // Setup commands for buttons
        public ICommand ShowBioCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ApproveCommand { get; }

        public PersonModel()
        {
            ShowBioCommand = new RelayCommand(_ => MessageBox.Show($"Bio for {Name}"));
            DeleteCommand = new RelayCommand(_ => MessageBox.Show($"Delete {Name}?"));
            ApproveCommand = new RelayCommand(_ => MessageBox.Show($"Approved {Name}"));
        }
    }
}
