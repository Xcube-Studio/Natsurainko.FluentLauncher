using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Views.Downloads.Instances;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;

internal partial class InstallViewModel : ObservableObject, INavigationAware
{
    private readonly GameService _gameService;
    private readonly DownloadService _downloadService;

    public InstallViewModel(GameService gameService, DownloadService downloadService)
    {
        _gameService = gameService;
        _downloadService = downloadService;
    }

    [ObservableProperty]
    public partial IEnumerable<InstanceLoaderItem> LoaderItems { get; set; } = [];

    [ObservableProperty]
    public partial IEnumerable<InstanceModItem> ModItems { get; set; } = [];

    [ObservableProperty]
    public partial VersionManifestItem CurrentInstance { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstanceIdValidity))]
    [NotifyPropertyChangedFor(nameof(CanInstall))]
    [NotifyCanExecuteChangedFor(nameof(InstallCommand))]
    public partial string InstanceId { get; set; } = null!;

    [ObservableProperty]
    public partial bool EnableIndependencyInstance { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InstanceIcon))]
    public partial List<InstanceLoaderItem> InstanceLoaderItems { get; set; } = [];

    [ObservableProperty]
    public partial List<InstanceModItem> InstanceModItems { get; set; } = [];

    [ObservableProperty]
    public partial bool LoadingMods { get; set; } = true;

    public bool InstanceIdValidity => !string.IsNullOrEmpty(InstanceId) && !_gameService.Games.Where(x => x.InstanceId.Equals(InstanceId)).Any();

    public bool CanInstall => InstanceIdValidity;

    public ImageSource InstanceIcon
    {
        get
        {
            if (InstanceLoaderItems != null && InstanceLoaderItems.Count != 0)
                return new BitmapImage(new Uri($"ms-appx:///Assets/Icons/furnace_front.png", UriKind.RelativeOrAbsolute));

            return new BitmapImage(new Uri(string.Format("ms-appx:///Assets/Icons/{0}.png", CurrentInstance.Type switch
            {
                "release" => "grass_block_side",
                "snapshot" => "crafting_table_front",
                "old_beta" => "dirt_path_side",
                "old_alpha" => "dirt_path_side",
                _ => "grass_block_side"
            }), UriKind.RelativeOrAbsolute));
        }
    }

    partial void OnInstanceLoaderItemsChanged(List<InstanceLoaderItem> value)
    {
        List<string> tags = new() { CurrentInstance.Id };

        if (value != null)
        {
            InstanceLoaderItem.ParseLoaderOrder(value, out var primaryLoader, out var secondaryLoader);

            if (primaryLoader != null)
                tags.Add($"{primaryLoader.Type}-{InstallPage.GetLoaderVersionFromInstallData(primaryLoader.SelectedInstallData)}");

            if (secondaryLoader != null)
                tags.Add($"{secondaryLoader.Type}-{InstallPage.GetLoaderVersionFromInstallData(secondaryLoader.SelectedInstallData)}");
        }

        InstanceId = string.Join('-', tags);
    }

    partial void OnInstanceModItemsChanged(List<InstanceModItem> value)
    {
        EnableIndependencyInstance = value.Count > 0;
    }

    [RelayCommand(CanExecute = nameof(CanInstall))]
    void Install()
    {
        InstanceLoaderItem.ParseLoaderOrder(InstanceLoaderItems, out var primaryLoader, out var secondaryLoader);
        InstanceInstallConfig installConfig = new()
        {
            EnableIndependencyInstance = EnableIndependencyInstance,
            InstanceId = InstanceId,
            ManifestItem = CurrentInstance,
            PrimaryLoader = primaryLoader,
            SecondaryLoader = secondaryLoader,
            AdditionalResources = [.. InstanceModItems!
                .Where(m => m.SelectedModrinthFile != null)
                .Select(m => new GameResourceFile(Task.FromResult(m.SelectedModrinthFile!.Url))
                {
                    FileName = m.SelectedModrinthFile.FileName,
                    Loaders = m.SelectedModrinthFile.Loaders,
                    Version = m.SelectedModrinthFile.McVersion
                })]
        };

        _downloadService.InstallInstance(installConfig);
        WeakReferenceMessenger.Default.Send(new GlobalNavigationMessage("Tasks/Download"));
    }

    [RelayCommand]
    void Loaded()
    {
        WeakReferenceMessenger.Default.Register<InstanceLoaderQueryMessage>(this, (s, m) =>
        {
            WeakReferenceMessenger.Default.Send(new InstanceLoaderSelectedMessage(InstanceLoaderItems));
        });
    }

    [RelayCommand]
    void Unloaded()
    {
        WeakReferenceMessenger.Default.Unregister<InstanceLoaderQueryMessage>(this);
    }

    async void INavigationAware.OnNavigatedTo(object? parameter)
    {
        CurrentInstance = parameter as VersionManifestItem
            ?? throw new InvalidDataException();
        InstanceId = CurrentInstance.Id;

        LoaderItems = InstanceLoaderItem.GetInstanceLoaderItems(CurrentInstance);
        ModItems = await InstanceModItem.GetInstanceModItemsAsync(CurrentInstance);
        LoadingMods = false;
    }
}
