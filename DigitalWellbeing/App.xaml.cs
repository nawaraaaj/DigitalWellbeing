using System.Configuration;
using System.Data;
using System.Windows;
using DigitalWellbeing.Data;

namespace DigitalWellbeing
{
    public partial class App : Application
    {
       protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DatabaseInitializer.Initialize();
        }
    }
}
