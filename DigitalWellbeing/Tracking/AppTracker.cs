using DigitalWellbeing.Services;
using System.Timers;
using System.Threading;

namespace DigitalWellbeing.Tracking
{
   public class AppTracker
    {
        private readonly System.Timers.Timer _timer;
        private readonly AppUsageService _appUsageService;
        private readonly DailySummaryService _dailySummaryService;

        private string _currentAppName;
        private DateTime _currentAppStartTime;
        private DateTime _lastTrackedDate;

        private bool _isTracking;

        public AppTracker()
        {
            _appUsageService = new AppUsageService();
            _dailySummaryService = new DailySummaryService();

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;

            _currentAppName = string.Empty;
            _currentAppStartTime = DateTime.Now;
            _lastTrackedDate = DateTime.Now;
        }

        public void StartTracking()
        {
            if (_isTracking)
                return;

            _isTracking = true;
            _currentAppStartTime = DateTime.Now;
            _currentAppName = Win32Api.GetActiveApplicationName();
            _lastTrackedDate = DateTime.Now;

            _timer.Start();
        }

        public void StopTracking()
        {
            if (!_isTracking)
                return;

            _timer.Stop();
            SaveCurrentAppUsage();
            _isTracking = false;
        }

        public void PauseTracking()
        {
            if (!_isTracking)
                return;

            _timer.Stop();
            SaveCurrentAppUsage();
        }

        public void ResumeTracking()
        {
            if (!_isTracking)
                return;

            _currentAppStartTime = DateTime.Now;
            _currentAppName = Win32Api.GetActiveApplicationName();
            _timer.Start();
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            HandleDateChange();

            string activeApp = Win32Api.GetActiveApplicationName();

            if (string.IsNullOrWhiteSpace(activeApp))
                return;

            if (_currentAppName == string.Empty)
            {
                _currentAppName = activeApp;
                _currentAppStartTime = DateTime.Now;
                return;
            }

            if (!activeApp.Equals(_currentAppName, StringComparison.OrdinalIgnoreCase))
            {
                SaveCurrentAppUsage();
                _currentAppName = activeApp;
                _currentAppStartTime = DateTime.Now;
            }
        }

        private void SaveCurrentAppUsage()
        {
            if (string.IsNullOrWhiteSpace(_currentAppName))
                return;

            int secondsUsed = (int)(DateTime.Now - _currentAppStartTime).TotalSeconds;

            if (secondsUsed <= 0)
                return;

            _appUsageService.AddAppUsage(_currentAppName,secondsUsed);

        }

        private void HandleDateChange()
        {
            if (DateTime.Today == _lastTrackedDate)
                return;

            SaveCurrentAppUsage();
            _lastTrackedDate = DateTime.Today;
            _currentAppStartTime = DateTime.Now;
        }

    }
}
