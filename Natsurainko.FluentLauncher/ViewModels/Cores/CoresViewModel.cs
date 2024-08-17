using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Download;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Experimental.GameManagement;
using Nrk.FluentCore.Experimental.GameManagement.Instances;
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
    private readonly SearchProviderService _searchProviderService;

    public ReadOnlyObservableCollection<MinecraftInstance> MinecraftInstances { get; init; }

    public CoresViewModel(
        GameService gameService,
        SettingsService settingsService,
        INavigationService navigationService,
        NotificationService notificationService,
        SearchProviderService searchProviderService)
    {
        _gameService = gameService;
        _settingsService = settingsService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _searchProviderService = searchProviderService;

        MinecraftInstances = _gameService.Games;

        (this as ISettingsViewModel).InitializeSettings();

        Task.Run(UpdateDisplayMinecraftInstances);
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
    private IEnumerable<MinecraftInstance> displayMinecraftInstances;

    public string DisplayFolderPath => (string.IsNullOrEmpty(ActiveMinecraftFolder) || !Directory.Exists(ActiveMinecraftFolder)) 
        ? ResourceUtils.GetValue("Cores", "CoresPage", "_FolderError")
        : ActiveMinecraftFolder;

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterIndex) ||
            e.PropertyName == nameof(SortByIndex))
            Task.Run(UpdateDisplayMinecraftInstances);
    }

    private void UpdateDisplayMinecraftInstances()
    {
        var infos = MinecraftInstances.Where(x =>
        {
            return FilterIndex switch
            {
                1 => x.Version.Type == MinecraftVersionType.Release,
                2 => x.Version.Type == MinecraftVersionType.Snapshot,
                3 => x.Version.Type == MinecraftVersionType.OldBeta || x.Version.Type == MinecraftVersionType.OldAlpha,
                _ => true
            };
        });

        var list = SortByIndex.Equals(0)
            ? infos.OrderBy(x => x.Name).ToList()
            : infos.OrderByDescending(x => x.GetConfig().LastLaunchTime).ToList();

        App.DispatcherQueue.TryEnqueue(() => DisplayMinecraftInstances = list);
    }

    IEnumerable<SearchProviderService.Suggestion> ProviderSuggestions(string searchText)
    {
        yield return new SearchProviderService.Suggestion
        {
            Title = ResourceUtils.GetValue("SearchSuggest", "_T1").Replace("{searchText}", searchText),
            Description = ResourceUtils.GetValue("SearchSuggest", "_D1"),
            InvokeAction = () => _navigationService.NavigateTo("Download/Navigation", new SearchOptions
            {
                SearchText = searchText,
                ResourceType = 1
            })
        };

        foreach (var item in MinecraftInstances)
        {
            if (item.InstanceId.Contains(searchText))
            {
                yield return SuggestionHelper.FromMinecraftInstance(item,
                    ResourceUtils.GetValue("SearchSuggest", "_D3"), 
                    () => _navigationService.NavigateTo("CoreManage/Navigation", item));
            }
        }
    }

    [RelayCommand]
    public void GoToSettings() => _navigationService.NavigateTo("Settings/Navigation", "Settings/Launch");

    [RelayCommand]
    public void GoToCoreSettings(MinecraftInstance MinecraftInstance) => _navigationService.NavigateTo("CoreManage/Navigation", MinecraftInstance);

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

        _navigationService.NavigateTo("Download/Navigation", new SearchOptions { ResourceType = 1 });
    }

    [RelayCommand]
    public void NavigateFolder()
    {
        if (Directory.Exists(ActiveMinecraftFolder))
            _ = Launcher.LaunchFolderPathAsync(ActiveMinecraftFolder);
    }

    [RelayCommand]
    void Loaded()
    {
        if (!_searchProviderService.ContainsSuggestionProvider(this))
            _searchProviderService.RegisterSuggestionProvider(this, ProviderSuggestions);
    }

    [RelayCommand]
    void Unloaded()
    {
        _searchProviderService.UnregisterSuggestionProvider(this);
    }
}
