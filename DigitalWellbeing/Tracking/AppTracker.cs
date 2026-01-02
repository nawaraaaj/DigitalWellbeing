using DigitalWellbeing.Services;
using System.Timers;
using System.Xml.Serialization;

namespace DigitalWellbeing.Tracking
{
    public class AppTracker
    {
        private readonly System.Timers.Timer _timer;
        private readonly AppUsageService _appusageService;
        private readonly DailySummaryService _dailySummaryService;

        private string _currentAppName;
        private DateTime _lastSwitchTime;
        private DateTime _lastTrackedDate;
        

        private int _accumulatedSeconds;
        private bool _isTracking;

        public AppTracker()
        {
            _appusageService = new AppUsageService();
            _dailySummaryService = new DailySummaryService();

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;

            _currentAppName = string.Empty;
            _lastSwitchTime = DateTime.Now;
            _lastTrackedDate = DateTime.Today;
            _accumulatedSeconds = 0;
        }

        public void StartTracking()
        {
            if (_isTracking)
                return;

            _isTracking = true;
            _currentAppName = Win32Api.GetActiveApplicationName();
            _lastSwitchTime = DateTime.Now;
            _lastTrackedDate = DateTime.Today;
            _accumulatedSeconds = 0;

            _timer.Start();
        }

        public void StopTracking()
        {
            if (!_isTracking)
                return;

            _timer.Stop();
            AccumulateTime();
            SaveCurrentAppUsage();
            _isTracking = false;
        }

        public void PauseTracking()
        {
            if (!_isTracking)
                return;

            _timer.Stop();
            AccumulateTime();
            SaveCurrentAppUsage();
        }

        public void ResumeTracking()
        {
            if (!_isTracking)
                return;

            _currentAppName = Win32Api.GetActiveApplicationName();
            _lastSwitchTime = DateTime.Now;
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
                _lastSwitchTime = DateTime.Now;
                return;
            }

            if (!activeApp.Equals(_currentAppName, StringComparison.OrdinalIgnoreCase))
            {
                AccumulateTime();
                SaveCurrentAppUsage();

                _currentAppName = activeApp;
                _lastSwitchTime = DateTime.Now;
            }
        }

        private void AccumulateTime()
        {
            int seconds = (int)(DateTime.Now - _lastSwitchTime).TotalSeconds;

            if (seconds > 0)
                _accumulatedSeconds += seconds;

            _lastSwitchTime = DateTime.Now;
        }
        private void SaveCurrentAppUsage()
        {
            if (string.IsNullOrWhiteSpace(_currentAppName))
                return;

            if (_accumulatedSeconds <= 0)
                return;

            _appusageService.AddAppUsage(_currentAppName, _accumulatedSeconds);

            _accumulatedSeconds = 0;
        }

        private void HandleDateChange()
        {
            if (DateTime.Today == _lastTrackedDate)
                return;

            AccumulateTime();
            SaveCurrentAppUsage();

            _lastTrackedDate = DateTime.Today;
            _lastSwitchTime = DateTime.Now;
        }
    }
}