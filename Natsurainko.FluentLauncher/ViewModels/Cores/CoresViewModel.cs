using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Cores;
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

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    public CoresViewModel(GameService gameService, SettingsService settingsService)
    {
        _gameService = gameService;
        _settingsService = settingsService;

        GameInfos = _gameService.GameInfos;

        (this as ISettingsViewModel).InitializeSettings();
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

    public ReadOnlyObservableCollection<GameInfo> GameInfos { get; init; }

    [ObservableProperty]
    private IEnumerable<GameInfo> displayGameInfos;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

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
            infos = infos.Where(x => x.AbsoluteId.Contains(SearchBoxInput));

        var list = SortByIndex.Equals(0)
            ? infos.OrderBy(x => x.AbsoluteId).ToList()
            : infos.OrderByDescending(x => x.GetSpecialConfig().LastLaunchTime).ToList();

        App.MainWindow.DispatcherQueue.TryEnqueue(() => DisplayGameInfos = list);
    }

    [RelayCommand]
    public void OpneCoreManage(GameInfo gameInfo) => Views.ShellPage.ContentFrame.Navigate(typeof(ManageNavigationPage), gameInfo);
}
