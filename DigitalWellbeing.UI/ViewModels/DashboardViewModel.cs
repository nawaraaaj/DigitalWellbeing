using DigitalWellbeing.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using DigitalWellbeing.Core.Services;

namespace DigitalWellbeing.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly AppUsageService _appUsageService;
        private readonly DispatcherTimer _refreshTimer;

        private DateTime _selectedDate = DateTime.Today;
        private readonly DateTime _minDate = DateTime.Today.AddDays(-7);

        // Date navigation
        public string SelectedDateLabel =>
            _selectedDate == DateTime.Today ? "Today" : _selectedDate.ToString("dddd, dd MMM yyyy");

        public bool CanGoBack => _selectedDate > _minDate;
        public bool CanGoForward => _selectedDate < DateTime.Today;

        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }

        // Screen time
        private int _totalScreenTimeSeconds;
        public string TotalTimeToday
        {
            get
            {
                var ts = TimeSpan.FromSeconds(_totalScreenTimeSeconds);
                if (ts.TotalHours >= 1)
                    return $"{(int)ts.TotalHours} hr, {ts.Minutes} min";
                return $"{ts.Minutes} min";
            }
        }

        // App list
        private ObservableCollection<AppUsage> _appUsages = new();
        public ObservableCollection<AppUsage> TodayAppUsages
        {
            get => _appUsages;
            set => SetProperty(ref _appUsages, value);
        }

        public DashboardViewModel()
        {
            _appUsageService = new AppUsageService();

            GoBackCommand = new RelayCommand(
                () => { _selectedDate = _selectedDate.AddDays(-1); RefreshAll(); },
                () => CanGoBack);

            GoForwardCommand = new RelayCommand(
                () => { _selectedDate = _selectedDate.AddDays(1); RefreshAll(); },
                () => CanGoForward);

            LoadUsage();

            // Auto-refresh only when viewing today
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += (s, e) => { if (_selectedDate == DateTime.Today) LoadUsage(); };
            _refreshTimer.Start();
        }

        private void RefreshAll()
        {
            OnPropertyChanged(nameof(SelectedDateLabel));
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            // Notify commands to re-evaluate CanExecute
            (GoBackCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (GoForwardCommand as RelayCommand)?.RaiseCanExecuteChanged();
            LoadUsage();
        }

        public void LoadTodayUsage() => LoadUsage();

        private void LoadUsage()
        {
            var usages = _appUsageService.GetUsageByDate(_selectedDate);

            TodayAppUsages.Clear();
            int total = 0;

            foreach (var usage in usages)
            {
                TodayAppUsages.Add(usage);
                total += usage.TimeUsedSeconds;
            }

            _totalScreenTimeSeconds = total;
            OnPropertyChanged(nameof(TotalTimeToday));
        }
    }
}