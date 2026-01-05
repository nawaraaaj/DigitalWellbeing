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
        private Tracking.AppTracker? _tracker;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Batteries.Init();
            DatabaseInitializer.Initialize();

            StartupManager.EnsureStartup();

            _tracker = new Tracking.AppTracker();
            _tracker.StartTracking();
        }
    }
}
