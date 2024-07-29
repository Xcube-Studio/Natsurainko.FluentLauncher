using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Management;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class CoresViewModel : ObservableObject, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;

    public ReadOnlyObservableCollection<GameInfo> GameInfos { get; init; }

    public CoresViewModel(
        GameService gameService,
        SettingsService settingsService,
        INavigationService navigationService,
        NotificationService notificationService)
    {
        _gameService = gameService;
        _settingsService = settingsService;
        _navigationService = navigationService;
        _notificationService = notificationService;

        GameInfos = _gameService.Games;

        (this as ISettingsViewModel).InitializeSettings();

        Task.Run(UpdateDisplayGameInfos);
        PropertyChanged += OnPropertyChanged;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFolderPath))]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    private string activeMinecraftFolder;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresFilterIndex))]
    private int filterIndex;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresSortByIndex))]
    private int sortByIndex;

    [ObservableProperty]
    private IEnumerable<GameInfo> displayGameInfos;

    public string DisplayFolderPath => (string.IsNullOrEmpty(ActiveMinecraftFolder) || !Directory.Exists(ActiveMinecraftFolder)) 
        ? ResourceUtils.GetValue("Cores", "CoresPage", "_FolderError")
        : ActiveMinecraftFolder;

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterIndex) ||
            e.PropertyName == nameof(SortByIndex))
            Task.Run(UpdateDisplayGameInfos);
    }

    private void UpdateDisplayGameInfos()
    {
        var infos = GameInfos.Where(x =>
        {
            return FilterIndex switch
            {
                1 => x.Type.Equals("release"),
                2 => x.Type.Equals("snapshot"),
                3 => x.Type.Contains("old"),
                _ => true
            };
        });

        var list = SortByIndex.Equals(0)
            ? infos.OrderBy(x => x.Name).ToList()
            : infos.OrderByDescending(x => x.GetConfig().LastLaunchTime).ToList();

        App.DispatcherQueue.TryEnqueue(() => DisplayGameInfos = list);
    }

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Launch");

    [RelayCommand]
    public void GoToCoreSettings(GameInfo gameInfo) => _navigationService.NavigateTo("CoreManage/Navigation", gameInfo);

    [RelayCommand]
    public void SearchAllMinecraft()
    {
        if (string.IsNullOrEmpty(_gameService.ActiveMinecraftFolder))
        {
            _notificationService.NotifyWithSpecialContent(
                ResourceUtils.GetValue("Notifications", "_NoMinecraftFolder"),
                "NoMinecraftFolderNotifyTemplate",
                GoToSettingsCommand, "\uE711");

            return;
        }

        _navigationService.NavigateTo("ResourcesSearchPage", new ResourceSearchData
        {
            SearchInput = string.Empty,
            ResourceType = 0
        });
    }

    [RelayCommand]
    public void NavigateFolder()
    {
        if (Directory.Exists(ActiveMinecraftFolder))
            _ = Launcher.LaunchFolderPathAsync(ActiveMinecraftFolder);
    }
}
