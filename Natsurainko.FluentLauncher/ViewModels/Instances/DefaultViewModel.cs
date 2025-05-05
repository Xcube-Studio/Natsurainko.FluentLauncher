using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Instances;
    
internal partial class DefaultViewModel : SettingsPageVM, ISettingsViewModel, INavigationAware
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly SearchProviderService _searchProviderService;
    private readonly INotificationService _notificationService;

    public ReadOnlyObservableCollection<MinecraftInstance> MinecraftInstances { get; init; }

    public DefaultViewModel(
        GameService gameService,
        SettingsService settingsService,
        INavigationService navigationService,
        SearchProviderService searchProviderService,
        INotificationService notificationService)
    {
        _gameService = gameService;
        _settingsService = settingsService;
        _navigationService = navigationService;
        _searchProviderService = searchProviderService;
        _notificationService = notificationService;

        MinecraftInstances = _gameService.Games;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayFolderPath))]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    public partial string ActiveMinecraftFolder { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresFilterIndex))]
    public partial int FilterIndex { get; set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresSortByIndex))]
    public partial int SortByIndex { get; set; }

    [ObservableProperty]
    public partial List<MinecraftInstance> DisplayMinecraftInstances { get; set; }

    public string DisplayFolderPath => (string.IsNullOrEmpty(ActiveMinecraftFolder) || !Directory.Exists(ActiveMinecraftFolder))
        ? LocalizedStrings.Instances_DefaultPage__FolderError
        : ActiveMinecraftFolder;

    partial void OnFilterIndexChanged(int value) => Task.Run(UpdateDisplayMinecraftInstances);

    partial void OnSortByIndexChanged(int value) => Task.Run(UpdateDisplayMinecraftInstances);

    void INavigationAware.OnNavigatedTo(object parameter) => Task.Run(UpdateDisplayMinecraftInstances);

    [RelayCommand]
    void GoToSettings() => GlobalNavigate("Settings/Navigation", "Settings/Launch");

    [RelayCommand]
    void InstallMinecraft() => GlobalNavigate("InstancesDownload/Navigation");

    [RelayCommand]
    void GoToCoreSettings(MinecraftInstance MinecraftInstance) => _navigationService.NavigateTo("Instances/Instance", MinecraftInstance);

    [RelayCommand]
    void NavigateFolder()
    {
        if (Directory.Exists(ActiveMinecraftFolder))
            _ = Launcher.LaunchFolderPathAsync(ActiveMinecraftFolder);
        else
        {
            _notificationService.Show(new ActionNotification
            {
                Title = LocalizedStrings.Notifications__NoMinecraftDataFolder,
                Message = LocalizedStrings.Notifications__NoMinecraftDataFolderDescription,
                Type = NotificationType.Warning,
                GetActionButton = () => new HyperlinkButton()
                {
                    Command = this.GoToSettingsCommand,
                    Content = LocalizedStrings.Instances_DefaultPage__GoToSettings
                }
            });
        }
    }

    IEnumerable<Suggestion> ProviderSuggestions(string searchText)
    {
        yield return new Suggestion
        {
            Title = LocalizedStrings.SearchSuggest__T1.Replace("{searchText}", searchText),
            Description = LocalizedStrings.SearchSuggest__D1,
            InvokeAction = () => GlobalNavigate("InstancesDownload/Navigation", searchText)
        };

        foreach (var item in MinecraftInstances.Where(i => i.InstanceId.Contains(searchText)))
            yield return SuggestionHelper.FromMinecraftInstance(item, LocalizedStrings.SearchSuggest__D3, () => GoToCoreSettings(item));
    }

    private void UpdateDisplayMinecraftInstances()
    {
        IEnumerable<MinecraftInstance> instances = MinecraftInstances.Where(x =>
        {
            return FilterIndex switch
            {
                1 => x.Version.Type == MinecraftVersionType.Release,
                2 => x.Version.Type == MinecraftVersionType.Snapshot,
                3 => x.Version.Type == MinecraftVersionType.OldBeta || x.Version.Type == MinecraftVersionType.OldAlpha,
                _ => true
            };
        });

        List<MinecraftInstance> list = SortByIndex.Equals(0)
            ? [.. instances.OrderBy(x => x.InstanceId)]
            : [.. instances.OrderByDescending(x => x.GetConfig().LastLaunchTime)];

        Dispatcher.TryEnqueue(() => DisplayMinecraftInstances = list);
    }

    protected override void OnLoaded()
    {
        if (!_searchProviderService.ContainsSuggestionProvider(this))
            _searchProviderService.RegisterSuggestionProvider(this, ProviderSuggestions);
    }

    protected override void OnUnloaded()
    {
        _searchProviderService.UnregisterSuggestionProvider(this);
    }
}
