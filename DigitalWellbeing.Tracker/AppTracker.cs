using DigitalWellbeing.Core.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.Timers;

namespace DigitalWellbeing.Tracker
{
    public class AppTracker
    {
        private readonly System.Timers.Timer _timer;
        private readonly AppUsageService _appusageService;
        private readonly object _stateLock = new();

        private string _currentAppName = string.Empty;
        private DateTime _lastSwitchTime;
        private DateTime _accumulatedDate;
        private double _accumulatedSeconds;
        private bool _isStarted;
        private bool _isPaused;
        private bool _isManuallyPaused;
        private bool _isSessionLocked;
        private bool _isSystemSuspended;

        public AppTracker()
        {
            _appusageService = new AppUsageService();

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;

        }

        public void StartTracking()
        {
            lock (_stateLock)
            {
                if (_isStarted)
                    return;

                _isStarted = true;
                SystemEvents.SessionSwitch += OnSessionSwitch;
                SystemEvents.PowerModeChanged += OnPowerModeChanged;
                _isSessionLocked = !Win32Api.IsInputDesktopAvailable();

                DateTime now = DateTime.Now;
                _lastSwitchTime = now;
                _accumulatedDate = now.Date;
                _currentAppName = Win32Api.GetActiveApplicationName() ?? string.Empty;
                _isPaused = _isSessionLocked;

                if (!_isPaused)
                    _timer.Start();
            }
        }

        public void StopTracking()
        {
            lock (_stateLock)
            {
                if (!_isStarted)
                    return;

                _timer.Stop();
                if (!_isPaused)
                {
                    AccumulateTime(DateTime.Now);
                    SaveCurrentAppUsage();
                }

                SystemEvents.SessionSwitch -= OnSessionSwitch;
                SystemEvents.PowerModeChanged -= OnPowerModeChanged;
                _isStarted = false;
                _currentAppName = string.Empty;
            }
        }

        public void PauseTracking()
        {
            lock (_stateLock)
            {
                if (!_isStarted || _isManuallyPaused)
                    return;

                _isManuallyPaused = true;
                try
                {
                    UpdatePausedState(DateTime.Now);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Session switch handling failed: {0}", ex);
                }
            }
        }

        public void ResumeTracking()
        {
            lock (_stateLock)
            {
                if (!_isStarted || !_isManuallyPaused)
                    return;

                _isManuallyPaused = false;
                try
                {
                    UpdatePausedState(DateTime.Now);
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Power mode handling failed: {0}", ex);
                }
            }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            lock (_stateLock)
            {
                if (!_isStarted)
                    return;

                if (e.Reason == SessionSwitchReason.SessionLock)
                    _isSessionLocked = true;
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                    _isSessionLocked = false;
                else
                    return;

                UpdatePausedState(DateTime.Now);
            }
        }

        private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
        {
            lock (_stateLock)
            {
                if (!_isStarted)
                    return;

                if (e.Mode == PowerModes.Suspend)
                    _isSystemSuspended = true;
                else if (e.Mode == PowerModes.Resume)
                    _isSystemSuspended = false;
                else
                    return;

                UpdatePausedState(DateTime.Now);
            }
        }

        private void UpdatePausedState(DateTime now)
        {
            bool shouldPause = _isManuallyPaused || _isSessionLocked || _isSystemSuspended;
            if (shouldPause == _isPaused)
                return;

            if (shouldPause)
            {
                _isPaused = true;
                _timer.Stop();
                AccumulateTime(now);
                SaveCurrentAppUsage();
                return;
            }

            SaveCurrentAppUsage();
            _currentAppName = Win32Api.GetActiveApplicationName() ?? string.Empty;
            _lastSwitchTime = now;
            _accumulatedDate = now.Date;
            _isPaused = false;
            _timer.Start();
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            lock (_stateLock)
            {
                if (!_isStarted || _isPaused)
                    return;

                try
                {
                    DateTime now = DateTime.Now;
                    string activeApp = Win32Api.GetActiveApplicationName() ?? string.Empty;
                    AccumulateTime(now);

                    if (!activeApp.Equals(_currentAppName, StringComparison.OrdinalIgnoreCase))
                    {
                        SaveCurrentAppUsage();
                        _currentAppName = activeApp;
                        _lastSwitchTime = now;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError("App usage tracking tick failed: {0}", ex);
                }
            }
        }

        private void AccumulateTime(DateTime now)
        {
            if (now <= _lastSwitchTime)
                return;

            DateTime intervalStart = _lastSwitchTime;
            while (intervalStart < now)
            {
                DateTime segmentDate = intervalStart.Date;
                DateTime segmentEnd = now < segmentDate.AddDays(1) ? now : segmentDate.AddDays(1);

                if (_accumulatedDate != segmentDate)
                {
                    SaveCurrentAppUsage();
                    _accumulatedDate = segmentDate;
                    _accumulatedSeconds = 0;
                }

                if (!string.IsNullOrWhiteSpace(_currentAppName))
                    _accumulatedSeconds += (segmentEnd - intervalStart).TotalSeconds;

                intervalStart = segmentEnd;
                _lastSwitchTime = intervalStart;
                if (intervalStart.Date != segmentDate)
                {
                    SaveCurrentAppUsage();
                    _accumulatedDate = intervalStart.Date;
                    _accumulatedSeconds = 0;
                }
            }

            _lastSwitchTime = now;
        }

        private void SaveCurrentAppUsage()
        {
            if (string.IsNullOrWhiteSpace(_currentAppName))
                return;

            if (_accumulatedSeconds < 1)
                return;

            int roundedSeconds = (int)Math.Round(_accumulatedSeconds);

            _appusageService.AddAppUsage(_currentAppName, roundedSeconds, _accumulatedDate);
            _accumulatedSeconds = 0;
        }
    }
}