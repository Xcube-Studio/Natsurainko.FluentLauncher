using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentCore.Extension;
using Natsurainko.FluentCore.Model.Install;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Cores.Properties;

partial class InformationViewModel : ObservableObject
{
    public InformationViewModel(GameCore core)
    {
        Core = core;
        core.LoadStatistic();

        Size = core.TotalSize.FormatSize();
        Assets = core.AssetsCount;
        Libraries = core.LibrariesCount;
        ModLoaders = core.ModLoaders;
        LastLaunchTime = core.CoreProfile.LastLaunchTime;

        TimeVisibility = LastLaunchTime == null
            ? Visibility.Collapsed
            : Visibility.Visible;

        LoaderVisibility = ModLoaders.Any()
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}

partial class InformationViewModel
{
    [ObservableProperty]
    private GameCore core;

    [ObservableProperty]
    private string size;

    [ObservableProperty]
    private int assets;

    [ObservableProperty]
    private int libraries;

    [ObservableProperty]
    private IEnumerable<ModLoaderInformation> modLoaders;

    [ObservableProperty]
    private DateTime? lastLaunchTime;

    [ObservableProperty]
    private Visibility timeVisibility;

    [ObservableProperty]
    private Visibility loaderVisibility;
}