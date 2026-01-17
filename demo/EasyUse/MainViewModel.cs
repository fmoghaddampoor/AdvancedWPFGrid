using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace EasyUse
{
    public class MainViewModel
    {
        public ObservableCollection<PersonModel> People { get; set; }
        public List<string> Roles { get; set; }

        public MainViewModel()
        {
            Roles = new List<string> { "User", "Admin", "Manager", "Guest" };
            People = new ObservableCollection<PersonModel>();

            // Add 10 rows of dummy data
            for (int i = 1; i <= 10; i++)
            {
                People.Add(new PersonModel
                {
                    Name = $"Person {i}",
                    Age = 20 + i,
                    IsActive = i % 2 == 0,
                    Role = Roles[i % Roles.Count],
                    Salary = 30000 + (i * 1000)
                });
            }
        }
    }
}
