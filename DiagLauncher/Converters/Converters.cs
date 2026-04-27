using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiagLauncher.Converters
{
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch { }
            }
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 120, 200));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ExeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string path) || !File.Exists(path))
                return null;

            // Tentative 1 : SHGetFileInfo (API Shell Windows — la plus fiable)
            try
            {
                var bs = ExtractViaShell(path);
                if (bs != null) return bs;
            }
            catch { }

            // Tentative 2 : ExtractAssociatedIcon + HICON direct
            try
            {
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon != null)
                {
                    var bs = HIconToBitmapSource(icon.Handle);
                    icon.Dispose();
                    if (bs != null) return bs;
                }
            }
            catch { }

            // Tentative 3 : ExtractAssociatedIcon + ToBitmap + PNG stream
            try
            {
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon != null)
                {
                    var bs = IconToPngStream(icon);
                    icon.Dispose();
                    if (bs != null) return bs;
                }
            }
            catch { }

            return null;
        }

        // ── Méthodes de conversion ────────────────────────────────────────────

        private static BitmapSource ExtractViaShell(string path)
        {
            var info = new SHFILEINFO();
            var ret = SHGetFileInfo(path, 0, ref info,
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(info),
                SHGFI_ICON | SHGFI_LARGEICON);

            if (ret == IntPtr.Zero || info.hIcon == IntPtr.Zero)
                return null;

            try
            {
                return HIconToBitmapSource(info.hIcon);
            }
            finally
            {
                DestroyIcon(info.hIcon);
            }
        }

        private static BitmapSource HIconToBitmapSource(IntPtr hIcon)
        {
            var bs = Imaging.CreateBitmapSourceFromHIcon(
                hIcon,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            bs.Freeze();
            return bs;
        }

        private static BitmapSource IconToPngStream(Icon icon)
        {
            using (var bmp = icon.ToBitmap())
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;
                var img = new BitmapImage();
                img.BeginInit();
                img.CacheOption  = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();
                img.Freeze();
                return img;
            }
        }

        // ── P/Invoke shell32 / user32 ─────────────────────────────────────────
        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON      = 0x100;
        private const uint SHGFI_LARGEICON = 0x000;

        [System.Runtime.InteropServices.StructLayout(
            System.Runtime.InteropServices.LayoutKind.Sequential,
            CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int    iIcon;
            public uint   dwAttributes;
            [System.Runtime.InteropServices.MarshalAs(
                System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [System.Runtime.InteropServices.MarshalAs(
                System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && !b;
    }

    public class EqualityConverter : System.Windows.Data.IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length == 2 && Equals(values[0], values[1]);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class FirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && s.Length > 0)
                return s[0].ToString().ToUpper();
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
