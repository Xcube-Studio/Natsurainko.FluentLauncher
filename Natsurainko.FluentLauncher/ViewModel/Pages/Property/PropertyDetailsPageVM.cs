using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.Shared.Mapping;
using ReactiveUI.Fody.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Property;

public class PropertyDetailsPageVM : ViewModelBase<Page>
{
    public PropertyDetailsPageVM(Page control) : base(control)
    {
    }

    [Reactive]
    public GameCoreViewData DisplayGameCore { get; set; }

    [Reactive]
    public double TotalSize { get; set; } = 0;

    [Reactive]
    public string ModLoaders { get; set; }

    [Reactive]
    public DateTime? LastLaunchTime { get; set; }

    [Reactive]
    public int? RawLibraryCount { get; set; }

    [Reactive]
    public int? RawAssetCount { get; set; }

    [Reactive]
    public string LibraryCount { get; set; }

    [Reactive]
    public string AssetCount { get; set; }

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LibraryCount))
            LibraryCount = ConfigurationManager.AppSettings.CurrentLanguage.GetString("PDP_Converter_Count")
                .Replace("{count}", RawLibraryCount.ToString());

        if (e.PropertyName != nameof(AssetCount))
            AssetCount = ConfigurationManager.AppSettings.CurrentLanguage.GetString("PDP_Converter_Count")
                .Replace("{count}", RawAssetCount.ToString());
    }

    public void Set(GameCoreViewData core)
    {
        DisplayGameCore = core;

        DispatcherHelper.RunAsync(async () =>
        {
            var information = await GameCoreLocator.GetGameCoreInformation(DisplayGameCore.Data.Root.FullName, DisplayGameCore.Data.Id);

            RawLibraryCount = information.LibraryCount;
            RawAssetCount = information.AssetCount;
            TotalSize = double.Parse(((double)information.TotalSize / (1024 * 1024)).ToString("0.00"));
            ModLoaders = information.ModLoaders.Any() ? string.Join('，', information.ModLoaders.Select(x => $"{x.LoaderType}：{x.Version}")) : null;
            LastLaunchTime = await GameCoreLocator.GetGameCoreLastLaunchTime(DisplayGameCore.Data.Root.FullName, DisplayGameCore.Data.Id);
        });
    }
}
