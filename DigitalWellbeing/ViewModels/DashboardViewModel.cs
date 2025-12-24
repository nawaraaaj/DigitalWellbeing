using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using DigitalWellbeing.Models;
using DigitalWellbeing.Services;

namespace DigitalWellbeing.ViewModels
{
    public class DashboardViewModel: BaseViewModel
    {
        private readonly AppUsageService _appUsageService;

        private int _totalScreenTimeSeconds;
        private int TotalScreenTimeSeconds
        {
            get => _totalScreenTimeSeconds;
            set => SetProperty(ref _totalScreenTimeSeconds, value);
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

            TotalScreenTimeSeconds = total;
        }
    }
}
