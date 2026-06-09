using DigitalWellbeing.Core.Data;
using DigitalWellbeing.ViewModels;
using System.Windows;

namespace DigitalWellbeing
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new DashboardViewModel();
        }
    }
}
