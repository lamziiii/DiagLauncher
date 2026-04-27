using DiagLauncher.Models;
using DiagLauncher.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DiagLauncher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private AppSettings _settings;
        private string _selectedCategory;
        private bool _isDarkTheme;

        public ObservableCollection<AppEntry> Apps { get; } = new ObservableCollection<AppEntry>();
        public ObservableCollection<AppEntry> FilteredApps { get; } = new ObservableCollection<AppEntry>();
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

        public ICommand LaunchAppCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand AddAppCommand { get; }
        public ICommand EditAppCommand { get; }
        public ICommand DeleteAppCommand { get; }
        public ICommand SelectCategoryCommand { get; }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                RefreshFilteredApps();
            }
        }

        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set { _isDarkTheme = value; OnPropertyChanged(nameof(IsDarkTheme)); }
        }

        public ThemeMode CurrentTheme
        {
            get => _settings.Theme;
            set
            {
                _settings.Theme = value;
                OnPropertyChanged(nameof(CurrentTheme));
                ApplyTheme();
                SaveSettings();
            }
        }

        public bool LaunchAtStartup
        {
            get => _settings.LaunchAtStartup;
            set
            {
                _settings.LaunchAtStartup = value;
                OnPropertyChanged(nameof(LaunchAtStartup));
                StartupService.SetStartup(value);
                SaveSettings();
            }
        }

        public MainViewModel()
        {
            _settings = ConfigService.Load();

            // Enregistrement automatique des apps bundlées (dossier assets\)
            if (BundledAppsService.SyncBundledApps(_settings))
                ConfigService.Save(_settings);

            LaunchAppCommand = new RelayCommand(LaunchApp);
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            AddAppCommand = new RelayCommand(_ => AddApp());
            EditAppCommand = new RelayCommand(EditApp);
            DeleteAppCommand = new RelayCommand(DeleteApp);
            SelectCategoryCommand = new RelayCommand(cat => SelectedCategory = cat as string);

            LoadApps();
            ApplyTheme();
        }

        private void LoadApps()
        {
            Apps.Clear();
            Categories.Clear();
            Categories.Add("Toutes");

            foreach (var app in _settings.Apps.OrderBy(a => a.Position))
                Apps.Add(app);

            foreach (var cat in _settings.Categories)
                Categories.Add(cat);

            SelectedCategory = "Toutes";
        }

        private void RefreshFilteredApps()
        {
            FilteredApps.Clear();
            var source = string.IsNullOrEmpty(SelectedCategory) || SelectedCategory == "Toutes"
                ? Apps
                : new ObservableCollection<AppEntry>(Apps.Where(a => a.Category == SelectedCategory));
            foreach (var app in source)
                FilteredApps.Add(app);
        }

        private void LaunchApp(object param)
        {
            if (!(param is AppEntry app)) return;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = app.ExecutablePath,
                    Arguments = app.Arguments ?? "",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible de lancer l'application.\n{ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSettings()
        {
            var dialog = new Views.SettingsDialog(this);
            dialog.ShowDialog();
        }

        private void AddApp()
        {
            var dialog = new Views.AddEditAppDialog(null, _settings.Categories);
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                dialog.Result.Position = Apps.Count;
                _settings.Apps.Add(dialog.Result);
                Apps.Add(dialog.Result);
                SaveSettings();
                RefreshFilteredApps();
            }
        }

        private void EditApp(object param)
        {
            if (!(param is AppEntry app)) return;
            var dialog = new Views.AddEditAppDialog(app, _settings.Categories);
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                app.Name = dialog.Result.Name;
                app.ExecutablePath = dialog.Result.ExecutablePath;
                app.Arguments = dialog.Result.Arguments;
                app.Category = dialog.Result.Category;
                app.ColorHex = dialog.Result.ColorHex;
                SaveSettings();
                RefreshFilteredApps();
            }
        }

        private void DeleteApp(object param)
        {
            if (!(param is AppEntry app)) return;
            var result = MessageBox.Show($"Supprimer \"{app.Name}\" ?", "Confirmation",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _settings.Apps.Remove(app);
                Apps.Remove(app);
                SaveSettings();
                RefreshFilteredApps();
            }
        }

        public void ApplyTheme()
        {
            IsDarkTheme = ThemeService.IsDark(_settings.Theme);
            var merged = Application.Current.Resources.MergedDictionaries;
            merged.Clear();
            var themeFile = IsDarkTheme ? "DarkTheme.xaml" : "LightTheme.xaml";
            merged.Add(new System.Windows.ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/Resources/{themeFile}", UriKind.Absolute)
            });
        }

        private void SaveSettings()
        {
            ConfigService.Save(_settings);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
