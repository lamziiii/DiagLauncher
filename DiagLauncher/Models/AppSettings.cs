using System.Collections.Generic;

namespace DiagLauncher.Models
{
    public enum ThemeMode
    {
        System,
        Dark,
        Light
    }

    public class AppSettings
    {
        public List<AppEntry> Apps { get; set; } = new List<AppEntry>();
        public ThemeMode Theme { get; set; } = ThemeMode.System;
        public bool LaunchAtStartup { get; set; } = false;
        public List<string> Categories { get; set; } = new List<string>
        {
            "Diagnostic", "Cartographie", "Reprogrammation", "Outils", "Autre"
        };
    }
}
