using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Class.Component;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class GameCoreViewData : ViewDataBase<GameCore>
{
    public GameCoreViewData(GameCore data) : base(data)
    {
        DispatcherHelper.RunAsync(() =>
        {
            Icon = new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", data.HasModLoader ? "furnace_front" : data.Type switch
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
                data.Id != data.Source ? $"{ConfigurationManager.AppSettings.CurrentLanguage.GetString("HP_Converter_Inherit")} {data.Source}" : data.Id
            });
        });
    }

    [Reactive]
    public BitmapImage Icon { get; private set; }

    [Reactive]
    public string Tag { get; private set; }

    public override int GetHashCode()
        => this.Data.Id.GetHashCode() ^ this.Data.Type.GetHashCode()
        ^ this.Data.JavaVersion.GetHashCode() ^ this.Data.Source.GetHashCode()
        ^ this.Data.HasModLoader.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj == null || obj is not GameCoreViewData)
            return false;

        var item = (GameCoreViewData)obj;

        return this.Data.Id == item.Data.Id
            && this.Data.Type == item.Data.Type
            && this.Data.JavaVersion == item.Data.JavaVersion
            && this.Data.Source == item.Data.Source
            && this.Data.HasModLoader == item.Data.HasModLoader;
    }
}
