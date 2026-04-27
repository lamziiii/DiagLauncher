using DiagLauncher.Models;
using Microsoft.Win32;

namespace DiagLauncher.Services
{
    public static class ThemeService
    {
        public static bool IsSystemDarkMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                if (value is int intVal)
                    return intVal == 0;
            }
            catch { }
            return false;
        }

        public static bool IsDark(ThemeMode mode)
        {
            return mode switch
            {
                ThemeMode.Dark => true,
                ThemeMode.Light => false,
                ThemeMode.System => IsSystemDarkMode(),
                _ => false
            };
        }
    }
}
