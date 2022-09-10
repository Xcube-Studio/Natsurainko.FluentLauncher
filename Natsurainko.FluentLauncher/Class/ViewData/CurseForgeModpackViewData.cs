using Natsurainko.FluentCore.Class.Model.Mod;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.Toolkits.Network;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class CurseForgeModpackViewData : ViewDataBase<CurseForgeModpack>
{
    public CurseForgeModpackViewData(CurseForgeModpack data) : base(data)
    {
        Tag = GetTag();
        _ = DownloadImageAsync();
    }

    [Reactive]
    public string CurrentVersion { get; set; }

    [Reactive]
    public string Tag { get; set; }

    [Reactive]
    public bool DownloadButtonEnable { get; set; }

    [Reactive]
    public BitmapImage Icon { get; set; }

    [Reactive]
    public List<CurseForgeModpackFileInfo> CurrentFileInfos { get; set; }

    [Reactive]
    public CurseForgeModpackFileInfo CurrentFileInfo { get; set; }

    public async Task DownloadImageAsync()
    {
        var res = await HttpWrapper.HttpGetAsync(Data.IconUrl);

        DispatcherHelper.RunAsync(async delegate
        {
            using var stream = await res.Content.ReadAsStreamAsync();

            Icon = new BitmapImage();
            await Icon.SetSourceAsync(stream.AsRandomAccessStream());

            res.Dispose();
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentVersion) && Data.Files.ContainsKey(CurrentVersion))
            CurrentFileInfos = Data.Files[CurrentVersion];

        if (e.PropertyName != nameof(DownloadButtonEnable))
            DownloadButtonEnable = CurrentFileInfo != null;
    }

    private string GetTag()
    {
        try
        {
            var modpack = Data;
            var timeSpan = DateTime.Now - modpack.LastUpdateTime;
            var types = new List<int>();

            modpack.Files.Values.ToList().ForEach(x => x.ForEach(y =>
            {
                if (y.ModLoaderType != null && !types.Contains((int)y.ModLoaderType))
                    types.Add((int)y.ModLoaderType);
            }));

            var modLoaderTypes = types.Select(x => x switch
            {
                0 => "All",
                1 => "Forge",
                2 => "Cauldron",
                3 => "LiteLoader",
                4 => "Fabric",
                _ => string.Empty
            }).Where(x => !string.IsNullOrEmpty(x));

            var builder = new StringBuilder()
                .Append(modLoaderTypes.Any() ? $"[{string.Join(',', modLoaderTypes)}]" : string.Empty);

            var timeBuilder = new StringBuilder()
                .Append(timeSpan.Days != 0 ? $"{timeSpan.Days} {ConfigurationManager.AppSettings.CurrentLanguage.GetString("RMP_Converter_Day")}" : string.Empty)
                .Append(timeSpan.Hours != 0 ? $" {timeSpan.Hours} {ConfigurationManager.AppSettings.CurrentLanguage.GetString("RMP_Converter_Hour")}" : string.Empty);

            string downloadCount = modpack.DownloadCount > 1000
                ? $"{modpack.DownloadCount / 1000}k"
                : modpack.DownloadCount.ToString();

            builder = builder
                .Append(builder.Length > 0 ? " " : string.Empty)
                .Append(modpack.SupportedVersions.Any() ? $"[{modpack.SupportedVersions.First()}{(modpack.SupportedVersions.First() == modpack.SupportedVersions.Last() ? string.Empty : $"-{modpack.SupportedVersions.Last()}")}]" : string.Empty)
                .Append(" ")
                .Append(ConfigurationManager.AppSettings.CurrentLanguage.GetString("RMP_Converter_Update").Replace("{time}", timeBuilder.ToString()))
                .Append(ConfigurationManager.AppSettings.CurrentLanguage.GetString("RMP_Converter_DownloadCount").Replace("{downloadCount}", downloadCount));

            return builder.ToString();
        }
        catch
        {
            return null;
        }
    }
}
