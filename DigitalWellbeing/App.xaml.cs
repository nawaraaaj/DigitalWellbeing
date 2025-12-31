using System.Configuration;
using System.Data;
using System.Windows;
using DigitalWellbeing.Data;
using SQLitePCL;

namespace DigitalWellbeing
{
    public partial class App : Application
    {
       protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Batteries.Init();
            DatabaseInitializer.Initialize();
        }
    }
}
