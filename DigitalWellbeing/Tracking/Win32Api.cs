using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DigitalWellbeing.Tracking
{
    public static class Win32Api
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        private const int MaxWindowTitleLength = 256;

        public static IntPtr GetActiveWindowHandle()
        {
            return GetForegroundWindow();
        }

        public static string GetActiveWindowTitle()
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
                return string.Empty;

            StringBuilder title = new StringBuilder(MaxWindowTitleLength);
            GetWindowText(handle, title, MaxWindowTitleLength);

            return title.ToString();
        }

        public static uint GetActiveProcessId()
        {
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
                return 0;

            GetWindowThreadProcessId(handle, out uint processId);
            return processId;
        }

        public static string GetActiveApplicationName()
        {
            try
            {
                uint pid = GetActiveProcessId();
                if (pid == 0)
                    return string.Empty;

                Process process = Process.GetProcessById((int)pid);
                return process.ProcessName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
