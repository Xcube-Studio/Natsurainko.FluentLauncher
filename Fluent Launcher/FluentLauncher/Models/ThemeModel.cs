using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentLauncher.Models
{
    public class ThemeModel
    {
        public BackgroundType BackgroundType { get; set; }

        public UniversalBrush Brush { get; set; } = new UniversalBrush();

        [JsonIgnore]
        public Brush EnabledBrush => BackgroundType switch
        {
            BackgroundType.Image => new ImageBrush()
            {
                Stretch = Stretch.UniformToFill,
                ImageSource = new BitmapImage(new Uri(File))
            },
            _ => Brush.GetBrush(),
        };

        public Color ThemeColor { get; set; }

        public ElementTheme ElementTheme { get; set; }

        public string Name { get; set; }

        public string File { get; set; }
    }

    public enum BackgroundType
    {
        Normal = 0,
        Acrylic = 1,
        Image = 2,
        Vedio = 3
    }
}
