using DigitalWellbeing.Core.Models;
using DigitalWellbeing.Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace DigitalWellbeing.ViewModels
{
    public class DayBarItem
    {
        public string DayLabel { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TotalSeconds { get; set; }
        public double BarHeight { get; set; }
        public bool IsSelected { get; set; }
        public ICommand? SelectCommand { get; set; }
        public bool HasNoData => TotalSeconds == 0;
        public string ToolTipLabel => $"{Date:ddd, dd MMM} — {(TotalSeconds == 0 ? "No data" : TimeSpan.FromSeconds(TotalSeconds).ToString(@"h\h\ m\m"))}";
    }

    public class DashboardViewModel : BaseViewModel
    {
        private readonly AppUsageService _appUsageService;
        private readonly DispatcherTimer _refreshTimer;

        private DateTime _selectedDate = DateTime.Today;

        public string SelectedDateLabel =>
            _selectedDate == DateTime.Today ? "Today" :
            _selectedDate == DateTime.Today.AddDays(-1) ? "Yesterday" :
            _selectedDate.ToString("dddd, dd MMM yyyy");

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

        private string _hourLabel1 = "0";
        private string _hourLabel2 = "0";
        private string _hourLabel3 = "0";

        public string HourLabel1
        {
            get => _hourLabel1;
            private set => SetProperty(ref _hourLabel1, value);
        }

        public string HourLabel2
        {
            get => _hourLabel2;
            private set => SetProperty(ref _hourLabel2, value);
        }

        public string HourLabel3
        {
            get => _hourLabel3;
            private set => SetProperty(ref _hourLabel3, value);
        }

        private ObservableCollection<AppUsage> _appUsages = new();
        public ObservableCollection<AppUsage> TodayAppUsages
        {
            get => _appUsages;
            set => SetProperty(ref _appUsages, value);
        }

        private ObservableCollection<DayBarItem> _weekBars = new();
        public ObservableCollection<DayBarItem> WeekBars
        {
            get => _weekBars;
            set => SetProperty(ref _weekBars, value);
        }

        public DashboardViewModel()
        {
            _appUsageService = new AppUsageService();
            LoadWeekBars();
            LoadUsage();

            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(5);
            _refreshTimer.Tick += (s, e) =>
            {
                if (_selectedDate == DateTime.Today)
                {
                    LoadWeekBars();
                    LoadUsage();
                }
            };
            _refreshTimer.Start();
        }

        private void SelectDate(DateTime date)
        {
            _selectedDate = date;
            OnPropertyChanged(nameof(SelectedDateLabel));
            LoadWeekBars();
            LoadUsage();
        }

        private void LoadWeekBars()
        {
            var bars = new ObservableCollection<DayBarItem>();
            var totals = new List<int>();

            for (int i = -6; i <= 0; i++)
            {
                var date = DateTime.Today.AddDays(i);
                var usages = _appUsageService.GetUsageByDate(date);
                int total = usages.Sum(u => u.TimeUsedSeconds);
                totals.Add(total);
            }

            int maxSeconds = totals.Max() > 0 ? totals.Max() : 1;

            int labelMax = Math.Max(maxSeconds, 1800);
            int step = labelMax / 3;
            int stepRounded = (int)(Math.Ceiling(step / 900.0) * 900);
            int chartMax = stepRounded * 3;

            HourLabel1 = FormatHourLabel(stepRounded);
            HourLabel2 = FormatHourLabel(stepRounded * 2);
            HourLabel3 = FormatHourLabel(chartMax);

            double maxBarHeight = 76.0;

            for (int i = -6; i <= 0; i++)
            {
                var date = DateTime.Today.AddDays(i);
                int total = totals[i + 6];
                var item = new DayBarItem
                {
                    DayLabel = date.ToString("ddd"),
                    Date = date,
                    TotalSeconds = total,
                    BarHeight = Math.Max(total > 0 ? 6 : 0, (total / (double)chartMax) * maxBarHeight),
                    IsSelected = date == _selectedDate
                };
                item.SelectCommand = new RelayCommand(() => SelectDate(item.Date));
                bars.Add(item);
            }

            WeekBars = bars;
        }

        private string FormatHourLabel(int seconds)
        {
            var ts = TimeSpan.FromSeconds(seconds);
            if (ts.TotalHours >= 1)
                return ts.Minutes == 0
                    ? $"{(int)ts.TotalHours}h"
                    : $"{(int)ts.TotalHours}h{ts.Minutes}m";
            return $"{ts.Minutes}m";
        }

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