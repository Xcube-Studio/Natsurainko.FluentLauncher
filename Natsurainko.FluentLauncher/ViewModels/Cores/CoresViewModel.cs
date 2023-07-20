using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Cores.InstallNewCore;
using Nrk.FluentCore.Classes.Datas.Launch;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class CoresViewModel : SettingsViewModelBase, ISettingsViewModel
{
    private static readonly Regex NameRegex = new("^[^/\\\\:\\*\\?\\<\\>\\|\"]{1,255}$");

    public ReadOnlyObservableCollection<GameInfo> GameInfos { get; init; }

    #region Settings

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresFilter))]
    private string filter;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresSortBy))]
    private string sortBy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenInstallCommand))]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    private string activeMinecraftFolder;

    [ObservableProperty]
    private GameInfo activeGameInfo;

    [SettingsProvider]
    private readonly SettingsService _settings;

    #endregion Settings

    private readonly GameService _gameService;

    private ListView ListView;

    public CoresViewModel(GameService gameService, SettingsService settings)
    {
        _settings = settings;
        _gameService = gameService;

        GameInfos = gameService.GameInfos;
        ActiveGameInfo = gameService.ActiveGameInfo;
    }

    [RelayCommand]
    private void Loaded(object parameter)
        => parameter.As<ListView, object>(e =>
        {
            ListView = e.sender;
            e.sender.ScrollIntoView(e.sender.SelectedItem);
        });

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(ActiveGameInfo) && ActiveGameInfo is not null)
            _gameService.ActivateGameInfo(ActiveGameInfo);
    }

    private void FilterAndSortGames()
    {/*
        IEnumerable<GameInfo> filtered = default;

        if (Filter.Equals("All"))
            filtered = GameInfos;
        else if (Filter.Equals("Release"))
            filtered = GameInfos.Where(x => x.Type.Equals("release"));
        else if (Filter.Equals("Snapshot"))
            filtered = GameInfos.Where(x => x.Type.Equals("snapshot"));
        else if (Filter.Equals("Old"))
            filtered = GameInfos.Where(x => x.Type.Contains("old_"));

        IEnumerable<GameInfo> list = default;

        list = SortBy == "Launch Date"
            ? filtered.OrderByDescending(x => x.CoreProfile.LastLaunchTime.GetValueOrDefault())
            : filtered.OrderBy(x => x.AbsoluteId);

        if (Search != null)
            list = list.Where(x => x.Id.ToLower().Contains(Search.ToLower()));*/
    }

    private bool EnableFolderCommand() => !string.IsNullOrEmpty(ActiveMinecraftFolder);

    //private bool EnableRenameCommand() => !string.IsNullOrEmpty(NewName) && NameRegex.Matches(NewName).Any() && !GameCores.Where(x => x.Id.Equals(NewName)).Any();

    #region Command 

    [RelayCommand]
    private void OpenFolder(GameInfo gameInfo) => _ = Launcher.LaunchFolderPathAsync(gameInfo.MinecraftFolderPath);

    [RelayCommand(CanExecute = nameof(EnableFolderCommand))]
    private void OpenInstall() => _ = new InstallCoreDialog
    {
        XamlRoot = Views.ShellPage._XamlRoot,
        InstalledCoreNames = GameInfos.Select(x => x.AbsoluteId).ToArray()
    }.ShowAsync();

    #endregion
}
