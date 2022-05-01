using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentLauncher.Models
{
    public class UniversalBrush
    {
        public BrushType BrushType { get; set; } = BrushType.Solid;

        public double TintLuminosityOpacity { get; set; } 

        public double TintOpacity { get; set; }

        public Color Color { get; set; } = Colors.Transparent;

        public Brush GetBrush() => BrushType switch
        {
            BrushType.Solid => new SolidColorBrush(Color),
            BrushType.Acyrlic => new AcrylicBrush
            {
                TintColor = Color,
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                TintLuminosityOpacity = TintLuminosityOpacity,
                FallbackColor = ((Models.ApplicationTheme)App.Current.Resources["ApplicationTheme"]).SystemBackgroundColor,
                TintOpacity = TintOpacity
            },
            _ => new SolidColorBrush(Color),
        };

        public override bool Equals(object obj)
        {
            if (typeof(UniversalBrush) != obj.GetType())
                return false;

            var item = (UniversalBrush)obj;
            if (this.BrushType == item.BrushType && this.TintLuminosityOpacity == item.TintLuminosityOpacity &&
                this.TintOpacity == item.TintOpacity && this.Color == item.Color)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.BrushType.GetHashCode() ^ this.Color.GetHashCode() ^ this.TintLuminosityOpacity.GetHashCode() ^ this.TintOpacity.GetHashCode();
        }
    }

    public enum BrushType
    {
        Solid = 0,
        Acyrlic = 1
    }
}
