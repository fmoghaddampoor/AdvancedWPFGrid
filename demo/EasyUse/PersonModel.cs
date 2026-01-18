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
    }
}
