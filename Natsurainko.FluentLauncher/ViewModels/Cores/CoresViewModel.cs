using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Classes.Data.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Cores;
using Natsurainko.FluentLauncher.Views.Downloads;
using Nrk.FluentCore.Classes.Datas.Launch;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class CoresViewModel : ObservableObject, ISettingsViewModel
{
    private readonly GameService _gameService;
    private bool initSettings = false;

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    public CoresViewModel(GameService gameService, SettingsService settingsService)
    {
        _gameService = gameService;
        _settingsService = settingsService;

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
    public void OpenCoreManage(GameInfo gameInfo) => Views.ShellPage.ContentFrame.Navigate(typeof(ManageNavigationPage), gameInfo);

    [RelayCommand]
    public void SearchAllMinecraft()
        => ShellPage.ContentFrame.Navigate(typeof(ResourcesSearchPage), new ResourceSearchData
        {
            SearchInput = string.Empty,
            ResourceType = 0
        });
}
