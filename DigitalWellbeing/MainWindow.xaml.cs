using DigitalWellbeing.Data;
using DigitalWellbeing.Tracking;
using DigitalWellbeing.ViewModels;
using System.Windows;

namespace DigitalWellbeing
{
    
    public partial class MainWindow : Window
    {
        private AppTracker _appTracker;
        public MainWindow()
        {
            InitializeComponent();

            DatabaseInitializer.Initialize();

            DataContext = new DashboardViewModel();

            _appTracker = new AppTracker();
            _appTracker.StartTracking();
        }
    }
}
