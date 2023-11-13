using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Launch;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class CoresViewModel : ObservableObject, ISettingsViewModel
{
    private bool initSettings = false;

    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;

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

        GameInfos = _gameService.GameInfos;

        (this as ISettingsViewModel).InitializeSettings();
        initSettings = true;

        Task.Run(UpdateDisplayGameInfos);
    }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresLayoutIndex))]
    private int segmentedSelectedIndex;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresFilterIndex))]
    private int filterIndex;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresSortByIndex))]
    private int sortByIndex;

    [ObservableProperty]
    private string searchBoxInput;

    public ReadOnlyObservableCollection<ExtendedGameInfo> GameInfos { get; init; }

    [ObservableProperty]
    private IEnumerable<ExtendedGameInfo> displayGameInfos;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!initSettings)
            return;

        if (e.PropertyName == nameof(FilterIndex) ||
            e.PropertyName == nameof(SortByIndex) ||
            e.PropertyName == nameof(SearchBoxInput))
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

        if (!string.IsNullOrEmpty(SearchBoxInput))
            infos = infos.Where(x => x.Name.ToLower().Contains(SearchBoxInput.ToLower()));

        var list = SortByIndex.Equals(0)
            ? infos.OrderBy(x => x.Name).ToList()
            : infos.OrderByDescending(x => x.LastLaunchTime).ToList();

        App.DispatcherQueue.TryEnqueue(() => DisplayGameInfos = list);
    }

    [RelayCommand]
    public void OpenCoreManage(GameInfo gameInfo)
        => _navigationService.NavigateTo("CoresManageNavigationPage", gameInfo);

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
    public void GoToSettings() => _navigationService.NavigateTo("SettingsNavigationPage", "LaunchSettingsPage");

}
