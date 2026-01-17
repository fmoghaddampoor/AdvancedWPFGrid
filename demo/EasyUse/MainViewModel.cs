using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace EasyUse
{
    public class MainViewModel
    {
        public ObservableCollection<PersonModel> People { get; set; }
        public List<string> Roles { get; set; }

        public MainViewModel(AppData appData)
        {
            Roles = appData.Roles ?? new List<string>();
            People = new ObservableCollection<PersonModel>(appData.People ?? new List<PersonModel>());
        }
    }
}
