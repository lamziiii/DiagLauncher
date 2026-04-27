using DiagLauncher.Models;
using DiagLauncher.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DiagLauncher.Views
{
    public partial class SettingsDialog : Window
    {
        private readonly MainViewModel _vm;
        private bool _loading = true;

        public SettingsDialog(MainViewModel vm)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            _vm = vm;

            // Thème
            switch (vm.CurrentTheme)
            {
                case ThemeMode.System: ThemeSystem.IsChecked = true; break;
                case ThemeMode.Dark:   ThemeDark.IsChecked   = true; break;
                case ThemeMode.Light:  ThemeLight.IsChecked  = true; break;
            }

            // Toggle démarrage
            SetToggle(vm.LaunchAtStartup, animate: false);

            _loading = false;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Theme_Checked(object sender, RoutedEventArgs e)
        {
            if (_loading) return;
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                _vm.CurrentTheme = tag switch
                {
                    "Dark"  => ThemeMode.Dark,
                    "Light" => ThemeMode.Light,
                    _       => ThemeMode.System
                };
            }
        }

        private void Toggle_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var newValue = !_vm.LaunchAtStartup;
            _vm.LaunchAtStartup = newValue;
            SetToggle(newValue, animate: true);
        }

        private void SetToggle(bool on, bool animate)
        {
            var accentBrush = Application.Current.TryFindResource("AccentColor") as SolidColorBrush;
            var offBrush    = Application.Current.TryFindResource("InputBorder") as SolidColorBrush;

            ToggleBorder.Background = on
                ? accentBrush ?? new SolidColorBrush(Color.FromRgb(30, 120, 200))
                : offBrush    ?? new SolidColorBrush(Color.FromRgb(48, 54, 61));

            double targetMargin = on ? 21 : 3;
            if (animate)
            {
                var anim = new ThicknessAnimation
                {
                    To       = new Thickness(targetMargin, 0, 0, 0),
                    Duration = System.TimeSpan.FromMilliseconds(150)
                };
                ToggleKnob.BeginAnimation(MarginProperty, anim);
            }
            else
            {
                ToggleKnob.Margin = new Thickness(targetMargin, 0, 0, 0);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
