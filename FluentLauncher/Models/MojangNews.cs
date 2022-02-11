using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentLauncher.Models
{
    public class MojangNews
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("entries")]
        public List<MojangNewsItem> Entries { get; set; }
    }

    public class MojangNewsItem
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("playPageImage")]
        public PlayPageImage PlayPageImage { get; set; }

        [JsonProperty("readMoreLink")]
        public string ReadMoreLink { get; set; }

        public BitmapImage BitmapImage { get; set; }
    }

    public class PlayPageImage
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
