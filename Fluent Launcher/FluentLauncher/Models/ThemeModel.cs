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

        public bool Deletable { get; set; } = true;

        public string Name { get; set; }

        public string File { get; set; }

        public override bool Equals(object obj)
        {
            if (typeof(ThemeModel) != obj.GetType())
                return false;

            var item = (ThemeModel)obj;
            if (this.BackgroundType == item.BackgroundType && this.Brush.Equals(item.Brush) &&
                this.ThemeColor == item.ThemeColor && this.ElementTheme == item.ElementTheme &&
                this.Name == item.Name && this.File == item.File)
                return true;

            return false;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.File.GetHashCode() ^ this.ElementTheme.GetHashCode() ^ this.ThemeColor.GetHashCode() ^ this.BackgroundType.GetHashCode() ^ this.Brush.GetHashCode() ^ this.ThemeColor.GetHashCode();
        }
    }

    public enum BackgroundType
    {
        Normal = 0,
        Acrylic = 1,
        Image = 2,
        Video = 3
    }
}
