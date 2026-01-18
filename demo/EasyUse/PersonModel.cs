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
        
        // Extended Properties
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = "General";
        public string Country { get; set; } = "Global";
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public double ExperienceYears { get; set; }
        public double Bonus { get; set; }
        public double Rating { get; set; }
        public bool IsRemote { get; set; }
        public double ProjectCount { get; set; }
    }
}
