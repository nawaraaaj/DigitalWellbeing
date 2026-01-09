using DigitalWellbeing.Tracker;
using System;
using System.Threading;

class Program
{
    private static Mutex? _mutex;
    static void Main(string[] args)
    {
        //single-instance
        _mutex = new Mutex(true, "DigitalWellbeing.Tracker", out bool isNewInstance);
        if (!isNewInstance)
            return;

        //starts on boot
        StartupManager.EnsureStartup();

        //start tracking
        var tracker = new AppTracker();
        tracker.StartTracking();

        //keep process alive
        Thread.Sleep(Timeout.Infinite);
    }
}