#pragma warning disable
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
        public double PerformanceScore { get; set; }
        
        // Setup commands for buttons
        public ICommand ShowBioCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ApproveCommand { get; }

        public PersonModel()
        {
            this.ShowBioCommand = new RelayCommand(_ => MessageBox.Show($"Bio for {this.Name}"));
            this.DeleteCommand = new RelayCommand(_ => MessageBox.Show($"Delete {this.Name}?"));
            this.ApproveCommand = new RelayCommand(_ => MessageBox.Show($"Approved {this.Name}"));
        }
    }
}
