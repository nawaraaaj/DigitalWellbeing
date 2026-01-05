using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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

        public static string? GetActiveApplicationName()
        {
           
            IntPtr handle = GetForegroundWindow();
            if (handle == IntPtr.Zero)
                return null;

            const int nChars = 256;
            StringBuilder titleBuffer = new StringBuilder(nChars);

            if (GetWindowText(handle, titleBuffer, nChars) > 0)
            {
                string title = titleBuffer.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(title))
                    return ExtractAppName(title);
            }
            GetWindowThreadProcessId(handle, out uint processId);
            if (processId == 0)
                return null;

            try
            {
                var process = Process.GetProcessById((int)processId);
                return process.ProcessName; 
            }
            catch
            {
                return null;
            }
        }

        private static string ExtractAppName(string windowTitle)
        {
            if (string.IsNullOrWhiteSpace(windowTitle))
                return null;

            string[] separators = { " - ", " — ", " | " };

            foreach (var sep in separators)
            {
                if (windowTitle.Contains(sep))
                {
                    var parts = windowTitle.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
                    return parts.Last().Trim();
                }
            }

            return windowTitle.Trim();
        }
    }
}
