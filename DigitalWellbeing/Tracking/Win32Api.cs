using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DigitalWellbeing.Tracking
{
    public static class Win32Api
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static string? GetActiveApplicationName()
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
                return null;

            GetWindowThreadProcessId(handle, out uint processId);
            if (processId == 0)
                return null;

            try
            {
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
