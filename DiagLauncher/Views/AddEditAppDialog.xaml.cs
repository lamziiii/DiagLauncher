using DiagLauncher.Converters;
using DiagLauncher.Models;
using DiagLauncher.Services;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiagLauncher.Views
{
    public partial class AddEditAppDialog : Window
    {
        public AppEntry Result { get; private set; }

        private static readonly string[] PaletteColors = new[]
        {
            "#1E78C8", "#2EA44F", "#E05252", "#F0A732",
            "#9C59D1", "#E67E22", "#1ABC9C", "#E91E63",
            "#607D8B", "#795548"
        };

        private string _selectedColor = PaletteColors[0];
        private readonly ExeToIconConverter _iconConverter = new ExeToIconConverter();
        private bool _nameEditedByUser = false;

        public AddEditAppDialog(AppEntry existing, List<string> categories)
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;

            // Catégories
            foreach (var cat in categories)
                CategoryBox.Items.Add(cat);
            if (CategoryBox.Items.Count > 0)
                CategoryBox.SelectedIndex = 0;

            CategoryBox.SelectionChanged += (s, e) => UpdatePreview();

            // Palette de couleurs
            foreach (var hex in PaletteColors)
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                var border = new Border
                {
                    Width        = 32,
                    Height       = 32,
                    Margin       = new Thickness(0, 0, 8, 8),
                    CornerRadius = new CornerRadius(16),
                    Background   = new SolidColorBrush(color),
                    Cursor       = System.Windows.Input.Cursors.Hand,
                    Tag          = hex
                };
                border.MouseLeftButtonUp += (s, ev) =>
                {
                    _selectedColor = (string)((Border)s).Tag;
                    HighlightSelectedColor();
                    UpdatePreview();
                };
                ColorPanel.Children.Add(border);
            }

            // Mode édition
            if (existing != null)
            {
                TitleText.Text     = "Modifier l'application";
                NameBox.Text       = existing.Name;
                PathBox.Text       = existing.ExecutablePath;
                ArgumentsBox.Text  = existing.Arguments;
                _selectedColor     = existing.ColorHex ?? PaletteColors[0];
                _nameEditedByUser  = true;

                if (!string.IsNullOrEmpty(existing.Category))
                    CategoryBox.SelectedItem = existing.Category;
            }

            HighlightSelectedColor();
            UpdatePreview();
        }

        // ─── Déplacement de la fenêtre ────────────────────────────────────────────
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        // ─── Reconnaissance au changement de chemin ───────────────────────────────
        private void BrowseExe_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title  = "Sélectionner l'exécutable",
                Filter = "Exécutables|*.exe;*.bat;*.cmd;*.lnk|Tous les fichiers|*.*"
            };
            if (dlg.ShowDialog() != true) return;

            PathBox.Text = dlg.FileName;
            TryAutoFill(dlg.FileName);
            UpdatePreview();
        }

        private void TryAutoFill(string path)
        {
            var result = AppRecognitionService.Recognize(path);

            // Nom — seulement si l'utilisateur n'a pas encore tapé manuellement
            if (!_nameEditedByUser || string.IsNullOrWhiteSpace(NameBox.Text))
            {
                NameBox.Text      = result.SuggestedName;
                _nameEditedByUser = false; // auto-rempli, pas utilisateur
            }

            // Catégorie
            CategoryBox.SelectedItem = result.SuggestedCategory;
            if (CategoryBox.SelectedIndex < 0)
                CategoryBox.SelectedIndex = 0;

            // Badge
            if (result.Recognized)
            {
                RecognizedText.Text        = $"Application reconnue : {result.SuggestedName}";
                RecognizedBadge.Visibility = Visibility.Visible;
            }
            else
            {
                RecognizedBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _nameEditedByUser = !string.IsNullOrEmpty(NameBox.Text);
            UpdatePreview();
        }

        // ─── Aperçu tuile ─────────────────────────────────────────────────────────
        private void UpdatePreview()
        {
            var brush = MakeBrush(_selectedColor);

            PreviewAccent.Fill    = brush;
            PreviewEllipse.Fill   = brush;
            PreviewLetter.Foreground = brush;

            var name = NameBox.Text;
            PreviewName.Text     = string.IsNullOrWhiteSpace(name) ? "Mon App" : name;
            PreviewCategory.Text = CategoryBox.SelectedItem as string ?? "";
            PreviewLetter.Text   = string.IsNullOrEmpty(name) ? "?" : name[0].ToString().ToUpper();

            // Icône
            var path = PathBox.Text;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                var img = _iconConverter.Convert(path, typeof(BitmapSource), null,
                                                 System.Globalization.CultureInfo.CurrentCulture)
                          as BitmapSource;
                PreviewIcon.Source    = img;
                PreviewLetter.Visibility = img != null ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                PreviewIcon.Source       = null;
                PreviewLetter.Visibility = Visibility.Visible;
            }
        }

        private void HighlightSelectedColor()
        {
            foreach (Border b in ColorPanel.Children)
            {
                bool selected = (string)b.Tag == _selectedColor;
                b.BorderThickness = new Thickness(selected ? 3 : 0);
                b.BorderBrush     = selected
                    ? new SolidColorBrush(Colors.White)
                    : null;
                b.Margin = selected
                    ? new Thickness(0, 0, 8, 8)
                    : new Thickness(0, 0, 8, 8);
            }
        }

        private static SolidColorBrush MakeBrush(string hex)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch
            {
                return new SolidColorBrush(Color.FromRgb(30, 120, 200));
            }
        }

        // ─── Validation & fermeture ───────────────────────────────────────────────
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Le nom est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(PathBox.Text))
            {
                MessageBox.Show("Le chemin de l'exécutable est obligatoire.", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = new AppEntry
            {
                Name           = NameBox.Text.Trim(),
                ExecutablePath = PathBox.Text.Trim(),
                Arguments      = ArgumentsBox.Text?.Trim() ?? "",
                Category       = CategoryBox.SelectedItem as string ?? "",
                ColorHex       = _selectedColor
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
