using System.Configuration;
using System.Data;
using System.Windows;
using DigitalWellbeing.Data;
using DigitalWellbeing.Helpers;
using DigitalWellbeing.Services;
using SQLitePCL;

namespace DigitalWellbeing
{
    public partial class App : System.Windows.Application
    {
        private Tracking.AppTracker? _tracker;
        private TrayIconManager? _trayManager;
        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

           DatabaseInitializer.Initialize();

            StartupManager.EnsureStartup();

            _tracker = new Tracking.AppTracker();
            _tracker.StartTracking();

            _trayManager = new TrayIconManager();

            if (e.Args.Contains("--background"))
            {
                ShowMainWindow();
            }
        }
        public void ShowMainWindow()
        {
            if (MainWindow == null)
            {
                MainWindow = new MainWindow();
                MainWindow.Closed += (s, e) =>
                {
                    MainWindow = null;
                };
            }
            MainWindow.Show();
            MainWindow.WindowState = WindowState.Normal;
            MainWindow.Activate();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _tracker?.StopTracking();
            _trayManager?.Dispose();
            base.OnExit(e);
        }
    }
}
