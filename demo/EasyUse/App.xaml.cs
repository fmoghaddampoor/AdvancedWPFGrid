using System.Windows;
using System.Text;
using System.IO;
using System;
using System.Text.Json;

namespace EasyUse
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += (s, args) =>
            {
                var ex = args.Exception;
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("========================================");
                messageBuilder.AppendLine($"Timestamp: {DateTime.Now}");
                messageBuilder.AppendLine("Unhandled Exception Detatils:");
                messageBuilder.AppendLine("========================================");
                
                int level = 0;
                while (ex != null)
                {
                    messageBuilder.AppendLine($"[Level {level++}]");
                    messageBuilder.AppendLine($"Exception Type: {ex.GetType().FullName}");
                    messageBuilder.AppendLine($"Message: {ex.Message}");
                    messageBuilder.AppendLine($"Stack Trace:\n{ex.StackTrace}");
                    messageBuilder.AppendLine(new string('-', 40));
                    ex = ex.InnerException;
                }
                
                string detailedErrorMessage = messageBuilder.ToString();
                try
                {
                    string crashDir = "CrashReports";
                    if (!Directory.Exists(crashDir))
                    {
                        Directory.CreateDirectory(crashDir);
                    }
                    string reportPath = Path.Combine(crashDir, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    File.WriteAllText(reportPath, detailedErrorMessage);
                }
                catch (Exception logEx)
                {
                    Console.Error.WriteLine($"Failed to write crash report: {logEx.Message}");
                }
                Console.Error.WriteLine(detailedErrorMessage);
                
                // Commented out to allow the app to actually crash and show the error clearly in the runlog
                // args.Handled = true;
            };
        }

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
                DataContext = viewModel,
            };

            mainWindow.Show();
        }
    }
}
