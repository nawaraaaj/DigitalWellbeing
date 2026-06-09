using DigitalWellbeing.Core.Data;

namespace DigitalWellbeing
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseInitializer.Initialize();

            var mainWindow = new MainWindow();
            mainWindow.Show();

        }
    }
}
