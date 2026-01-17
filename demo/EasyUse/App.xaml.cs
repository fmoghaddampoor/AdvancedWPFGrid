using System.Windows;
using System.Text.Json;

namespace EasyUse
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppData? appData = null;
            try
            {
                string jsonPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "data.json");
                if (System.IO.File.Exists(jsonPath))
                {
                    string jsonString = System.IO.File.ReadAllText(jsonPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    appData = JsonSerializer.Deserialize<AppData>(jsonString, options);
                }
                else
                {
                    MessageBox.Show($"Data file not found at: {jsonPath}");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }

            // Fallback if data loading fails
            appData ??= new AppData();

            var viewModel = new MainViewModel(appData);
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            mainWindow.Show();
        }
    }
}
