using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace FluentLauncher.Models
{
    public class ApplicationTheme : INotifyPropertyChanged
    {
        private Color _systemBackgroundColor = Colors.Transparent;

        public Color SystemBackgroundColor { get => _systemBackgroundColor; set { _systemBackgroundColor = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
