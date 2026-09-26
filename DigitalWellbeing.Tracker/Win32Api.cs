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

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint OpenInputDesktop(uint flags, bool inherit, uint desiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseDesktop(nint desktop);

        private const uint DesktopSwitchDesktop = 0x0100;

        public static bool IsInputDesktopAvailable()
        {
            nint desktop = OpenInputDesktop(0, false, DesktopSwitchDesktop);
            if (desktop == nint.Zero)
                return false;

            CloseDesktop(desktop);
            return true;
        }

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
                using var process = Process.GetProcessById((int)processId);
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
