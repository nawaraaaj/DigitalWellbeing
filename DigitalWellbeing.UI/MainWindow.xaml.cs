using DigitalWellbeing.Data;
using DigitalWellbeing.Tracking;
using DigitalWellbeing.ViewModels;
using System.Windows;

namespace DigitalWellbeing
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DatabaseInitializer.Initialize();

            DataContext = new DashboardViewModel();
        }
    }
}
