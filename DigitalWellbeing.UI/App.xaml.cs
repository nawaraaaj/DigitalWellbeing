using DigitalWellbeing.Core.Data;
using DigitalWellbeing.Tracker;
using System.Windows;

namespace DigitalWellbeing
{
    public partial class App : System.Windows.Application
    {
        private AppTracker? _tracker;

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseInitializer.Initialize();

            var mainWindow = new MainWindow();
            mainWindow.Show();

        }
    }
}
