using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DigitalWellbeing.Tracker
{
    public static class Win32Api
    {
        [DllImport("user32.dll")]
        private static extern nint GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(nint hWnd, out uint processId);

        public static string? GetActiveApplicationName()
        {
            nint handle = GetForegroundWindow();
            if (handle == nint.Zero)
                return null;

            GetWindowThreadProcessId(handle, out uint processId);
            if (processId == 0)
                return null;

            try
            {
                // returns user-friendly app name from app-metadata or process-name
                var process = Process.GetProcessById((int)processId);
                string? friendlyName = process.MainModule?.FileVersionInfo?.FileDescription;
                return !string.IsNullOrEmpty(friendlyName) ? friendlyName : process.ProcessName;
            }
            catch
            {
                return null;
            }
        }
    }
}
