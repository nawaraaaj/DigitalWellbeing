using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DigitalWellbeing.Models;
using DigitalWellbeing.Services;
using System.Windows.Threading;
using System.Security.Principal;

namespace DigitalWellbeing.ViewModels
{
    public class DashboardViewModel: BaseViewModel
    {
        private readonly AppUsageService _appUsageService;
        private readonly DispatcherTimer _refreshTimer;

        public string TodayDate => DateTime.Today.ToString("dddd, dd MMM yyyy");


        private int _totalScreenTimeSeconds;
        private int TotalScreenTimeSeconds
        {
            get => _totalScreenTimeSeconds;
            set => SetProperty(ref _totalScreenTimeSeconds, value);
        }

        public string TotalTimeToday
        {
            get
            {
                var ts = TimeSpan.FromSeconds(_totalScreenTimeSeconds);
                return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}"; 
            }
        }


        private ObservableCollection<AppUsage> _todayAppUsages = new ObservableCollection<AppUsage>();
        public ObservableCollection<AppUsage> TodayAppUsages
        {
            get => _todayAppUsages;
            set => SetProperty(ref _todayAppUsages, value);
        }

        public DashboardViewModel()
        {
            _appUsageService = new AppUsageService();
            TodayAppUsages = new ObservableCollection<AppUsage>();

            LoadTodayUsage();

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += (s, e) => LoadTodayUsage();
            _refreshTimer.Start();
        }

        public void LoadTodayUsage()
        {
            var usages = _appUsageService.GetTodayUsage();

            TodayAppUsages.Clear();
            int total = 0;

            foreach( var usage in usages)
            {
                TodayAppUsages.Add(usage);
                total += usage.TimeUsedSeconds;
            }

            _totalScreenTimeSeconds = total;
            OnPropertyChanged(nameof(TotalTimeToday));
        }
    }
}
