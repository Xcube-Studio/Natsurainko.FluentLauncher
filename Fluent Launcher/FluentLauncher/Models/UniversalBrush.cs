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
        public BrushType BrushType;

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
                TintOpacity = TintOpacity
            },
            _ => new SolidColorBrush(Color),
        };
    }

    public enum BrushType
    {
        Solid = 0,
        Acyrlic = 1
    }
}
