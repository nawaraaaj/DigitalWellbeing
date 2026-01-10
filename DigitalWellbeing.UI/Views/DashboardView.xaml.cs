using DigitalWellbeing.ViewModels;

namespace DigitalWellbeing.Views
{
    public partial class DashboardView : System.Windows.Controls.UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel();
        }
    }
}
