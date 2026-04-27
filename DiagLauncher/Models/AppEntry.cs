using System.ComponentModel;

namespace DiagLauncher.Models
{
    public class AppEntry : INotifyPropertyChanged
    {
        private string _name;
        private string _executablePath;
        private string _arguments;
        private string _category;
        private string _colorHex;
        private int _position;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string ExecutablePath
        {
            get => _executablePath;
            set { _executablePath = value; OnPropertyChanged(nameof(ExecutablePath)); }
        }

        public string Arguments
        {
            get => _arguments;
            set { _arguments = value; OnPropertyChanged(nameof(Arguments)); }
        }

        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        public string ColorHex
        {
            get => _colorHex;
            set { _colorHex = value; OnPropertyChanged(nameof(ColorHex)); }
        }

        public int Position
        {
            get => _position;
            set { _position = value; OnPropertyChanged(nameof(Position)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
