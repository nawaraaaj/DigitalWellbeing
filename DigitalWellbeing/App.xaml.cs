using System.Configuration;
using System.Data;
using System.Windows;
using DigitalWellbeing.Data;
using DigitalWellbeing.Helpers;
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

            StartupManager.EnsureStartup();

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            var tracker = new Tracking.AppTracker();
            tracker.StartTracking();

            var summaryService = new Services.DailySummaryService();
            summaryService.GenerateOrUpdateDailySummary();
        }
    }
}
