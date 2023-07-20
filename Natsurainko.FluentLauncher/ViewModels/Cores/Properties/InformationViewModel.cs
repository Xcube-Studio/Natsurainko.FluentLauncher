using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Data.Launch;
using Nrk.FluentCore.Classes.Datas.Launch;
using Nrk.FluentCore.Utils;
using System;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Cores.Properties;

partial class InformationViewModel : ObservableObject
{
    public GameStatisticInfo GameStatisticInfo { get; set; }

    public GameInfo Game { get; set; }

    public DateTime? LastLaunchTime { get; set; }

    public Visibility TimeVisibility { get; set; }

    public Visibility LoaderVisibility { get; set; }

    public InformationViewModel(GameInfo game)
    {
        Game = game;
        GameStatisticInfo = game.GetStatisticInfo();
        var specialConfig = game.GetSpecialConfig();

        LastLaunchTime = specialConfig.LastLaunchTime;
        TimeVisibility = LastLaunchTime == null
            ? Visibility.Collapsed
            : Visibility.Visible;

        LoaderVisibility = (GameStatisticInfo.ModLoaders?.Any()).GetValueOrDefault()
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}