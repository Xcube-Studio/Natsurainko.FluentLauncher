using Natsurainko.FluentCore.Class.Model.Install.Vanilla;
using Natsurainko.FluentLauncher.Class.Component;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace Natsurainko.FluentLauncher.Class.ViewData
{
    public class CoreManifestItemViewData : ViewDataBase<CoreManifestItem>
    {
        public CoreManifestItemViewData(CoreManifestItem data) : base(data)
        {
            DispatcherHelper.RunAsync(() =>
            {
                Icon = new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", data.Type switch
                {
                    "release" => "grass_block_side",
                    "snapshot" => "crafting_table_front",
                    "old_beta" => "dirt_path_side",
                    "old_alpha" => "dirt_path_side",
                    _ => "grass_block_side"
                }), UriKind.RelativeOrAbsolute));

                Tag = string.Join(' ', new List<string>
                {
                    ConfigurationManager.AppSettings.CurrentLanguage.GetString($"HP_Converter_{data.Type}"),
                    data.Id
                });
            });
        }

        [Reactive]
        public BitmapImage Icon { get; private set; }

        [Reactive]
        public string Tag { get; private set; }

        public DateTime? DateTime => Environment.OSVersion.Version.Build < 22000 ? null : System.DateTime.Parse(this.Data.ReleaseTime);
    }
}
