﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores;
    
internal partial class DefaultViewModel : ObservableObject, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settingsService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly SearchProviderService _searchProviderService;
    private readonly NotificationService _notificationService;

    public ReadOnlyObservableCollection<MinecraftInstance> MinecraftInstances { get; init; }

    public DefaultViewModel(
        GameService gameService,
        SettingsService settingsService,
        INavigationService navigationService,
        SearchProviderService searchProviderService,
        NotificationService notificationService)
    {
        _gameService = gameService;
        _settingsService = settingsService;
        _navigationService = navigationService;
        _searchProviderService = searchProviderService;
        _notificationService = notificationService;

        MinecraftInstances = _gameService.Games;

        (this as ISettingsViewModel).InitializeSettings();

        Task.Run(UpdateDisplayMinecraftInstances);
        PropertyChanged += OnPropertyChanged;
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
        ? LocalizedStrings.Cores_DefaultPage__FolderError
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

        List<MinecraftInstance> list = SortByIndex.Equals(0)
            ? [.. infos.OrderBy(x => x.InstanceId)]
            : [..infos.OrderByDescending(x => x.GetConfig().LastLaunchTime)];

        App.DispatcherQueue.TryEnqueue(() => DisplayMinecraftInstances = list);
    }

    IEnumerable<Suggestion> ProviderSuggestions(string searchText)
    {
        yield return new Suggestion
        {
            Title = LocalizedStrings.SearchSuggest__T1.Replace("{searchText}", searchText),
            Description = LocalizedStrings.SearchSuggest__D1,
            InvokeAction = () => WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("InstancesDownload/Navigation", searchText))
        };

        foreach (var item in MinecraftInstances.Where(i => i.InstanceId.Contains(searchText)))
            yield return SuggestionHelper.FromMinecraftInstance(item, LocalizedStrings.SearchSuggest__D3, () => GoToCoreSettings(item));
    }

    [RelayCommand]
    void GoToSettings() => WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("Settings/Navigation", "Settings/Launch"));

    [RelayCommand]
    void GoToCoreSettings(MinecraftInstance MinecraftInstance) => _navigationService.NavigateTo("Cores/Instance", MinecraftInstance);

    [RelayCommand]
    void InstallMinecraft() => WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("InstancesDownload/Navigation"));

    [RelayCommand]
    void NavigateFolder()
    {
        if (Directory.Exists(ActiveMinecraftFolder))
            _ = Launcher.LaunchFolderPathAsync(ActiveMinecraftFolder);
        else 
            _notificationService.NotifyWithSpecialContent(
                LocalizedStrings.Notifications__NoMinecraftFolder,
                "NoMinecraftFolderNotifyTemplate",
                GoToSettingsCommand, "\uE711");
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
