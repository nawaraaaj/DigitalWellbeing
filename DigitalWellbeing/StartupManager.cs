using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace DigitalWellbeing.Helpers
{

    [SupportedOSPlatform("windows")]
    public static class StartupManager
    {
        private const string AppName = "DigitalWellbeing";

        public static void EnsureStartup()
        {
            string exePath = Process.GetCurrentProcess().MainModule!.FileName!;

            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", writable: true))

            if (key != null)
            {
                object? existingValue = key.GetValue(AppName);

                if(existingValue == null || existingValue.ToString() != exePath)
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
        }
    }
}