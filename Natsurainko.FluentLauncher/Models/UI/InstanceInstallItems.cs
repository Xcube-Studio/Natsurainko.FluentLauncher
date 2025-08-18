using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Experimental.GameManagement.Modpacks;
using Nrk.FluentCore.GameManagement.Installer;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Models.UI;

internal partial class InstanceLoaderItem : ObservableObject
{
    public required ModLoaderType Type { get; init; }

    [ObservableProperty]
    public partial object[]? InstallDatas { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial object SelectedInstallData { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(Text))]
    public partial bool Supported { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(Text))]
    public partial bool Conflicted { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    public partial bool Loading { get; set; } = true;

    public bool IsEnabled => Supported && !Conflicted;

    public string Text
    {
        get
        {
            if (Loading)
                return LocalizedStrings.Downloads_Instances_InstallPage__Loading;

            if (!Supported)
                return LocalizedStrings.Downloads_Instances_InstallPage__NotSupported;

            if (Conflicted)
                return LocalizedStrings.Downloads_Instances_InstallPage__Conflicted;

            return string.Empty;
        }
    }

    public string Description => LocalizedStrings.GetString($"Downloads_Instances_InstallPage__{Type}");

    [RelayCommand]
    void Loaded()
    {
        WeakReferenceMessenger.Default.Register<InstanceLoaderSelectedMessage>(this, (s, m) =>
        {
            var list = m.Value.Select(x => x.Type).ToList();

            foreach (var modLoader in GetConflictedLoaders())
            {
                if (list.Contains(modLoader))
                {
                    Conflicted = true;
                    return;
                }
            }

            Conflicted = false;
        });
    }

    [RelayCommand]
    void Unloaded()
    {
        WeakReferenceMessenger.Default.Unregister<InstanceLoaderSelectedMessage>(this);
    }

    private ModLoaderType[] GetConflictedLoaders() => Type switch
    {
        ModLoaderType.Forge => [ModLoaderType.NeoForge, ModLoaderType.Fabric, ModLoaderType.Quilt],
        ModLoaderType.NeoForge => [ModLoaderType.Forge, ModLoaderType.Fabric, ModLoaderType.Quilt, ModLoaderType.OptiFine],
        ModLoaderType.Fabric => [ModLoaderType.NeoForge, ModLoaderType.Forge, ModLoaderType.Quilt, ModLoaderType.OptiFine],
        ModLoaderType.Quilt => [ModLoaderType.NeoForge, ModLoaderType.Fabric, ModLoaderType.Forge, ModLoaderType.OptiFine],
        ModLoaderType.OptiFine => [ModLoaderType.Fabric, ModLoaderType.Quilt, ModLoaderType.NeoForge],
        _ => []
    };

    public bool GetTextLoad(string text) => !string.IsNullOrWhiteSpace(text);

    public static InstanceLoaderItem[] GetInstanceLoaderItems(VersionManifestItem manifestItem)
    {
        ModLoaderType[] loaders = [ModLoaderType.NeoForge, ModLoaderType.Forge, ModLoaderType.OptiFine, ModLoaderType.Fabric, ModLoaderType.Quilt];

        return [.. loaders.Select(loader =>
        {
            InstanceLoaderItem item = new() { Type = loader };

            Task.Run(async () =>
            {
                var httpClient = App.GetService<HttpClient>();
                var downloadService = App.GetService<DownloadService>();
                object[] installDatas = [];

                try
                {
                    installDatas = loader switch
                    {
                        ModLoaderType.NeoForge => await ForgeInstallDataApi.GetNeoForgeInstallDataAsync(manifestItem.Id, httpClient, downloadService.DownloadMirror),
                        ModLoaderType.Forge => await ForgeInstallDataApi.GetForgeInstallDataAsync(manifestItem.Id, httpClient, downloadService.DownloadMirror),
                        ModLoaderType.OptiFine => await OptiFineInstallDataApi.GetOptiFineInstallDataFromBmclApiAsync(manifestItem.Id, httpClient),
                        ModLoaderType.Fabric => await FabricInstallDataApi.GetFabricInstallDataAsync(manifestItem.Id, httpClient),
                        ModLoaderType.Quilt => await QuiltInstallDataApi.GetQuiltInstallDataAsync(manifestItem.Id, httpClient),
                        _ => throw new NotImplementedException()
                    };
                }
                catch (Exception ex)
                {
                    //TODO: Log Exception
                }

                await App.DispatcherQueue.EnqueueAsync(() =>
                {
                    item.InstallDatas = installDatas;
                    item.Supported = installDatas.Length != 0;
                    item.Loading = false;

                    if (installDatas.Length > 0)
                        item.SelectedInstallData = installDatas[0];
                });
            });

            return item;
        })];
    }

    public static void ParseLoaderOrder(List<InstanceLoaderItem>? items, out InstanceLoaderItem? primaryLoader, out InstanceLoaderItem? secondaryLoader)
    {
        if (items == null || items.Count == 0)
        {
            primaryLoader = null;
            secondaryLoader = null;

            return;
        }

        if (items.Count == 1)
        {
            primaryLoader = items[0];
            secondaryLoader = null;

            return;
        }

        primaryLoader = items.First(x => x.Type != ModLoaderType.OptiFine);
        secondaryLoader = items.First(x => x.Type == ModLoaderType.OptiFine);
    }
}

internal partial class InstanceModItem : ObservableObject
{
    public required ModrinthProject ModrinthProject { get; init; }

    [ObservableProperty]
    public partial ModrinthFile? SelectedModrinthFile { get; set; }

    [ObservableProperty]
    public partial IEnumerable<ModrinthFile>? FilteredFiles { get; set; }

    [ObservableProperty]
    public partial IEnumerable<ModrinthFile>? Files { get; set; }

    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(Text))]
    public partial bool Supported { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnabled))]
    [NotifyPropertyChangedFor(nameof(Text))]
    public partial bool Conflicted { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Text))]
    public partial bool Loading { get; set; } = true;

    public bool IsEnabled => Supported && !Conflicted;

    public string Text
    {
        get
        {
            if (Loading)
                return LocalizedStrings.Downloads_Instances_InstallPage__Loading;

            if (!Supported)
                return LocalizedStrings.Downloads_Instances_InstallPage__ModNotSupported;

            if (Conflicted)
                return LocalizedStrings.Downloads_Instances_InstallPage__Conflicted;

            return string.Empty;
        }
    }

    [RelayCommand]
    void Loaded()
    {
        WeakReferenceMessenger.Default.Register<InstanceLoaderSelectedMessage>(this, (s, m) => UpdateSelection(m.Value));
    }

    [RelayCommand]
    void Unloaded()
    {
        WeakReferenceMessenger.Default.Unregister<InstanceLoaderSelectedMessage>(this);
    }

    void UpdateSelection(List<InstanceLoaderItem> items)
    {
        if (items.Count == 0)
        {
            Conflicted = false;
            IsSelected = false;
            FilteredFiles = Files;
            return;
        }

        InstanceLoaderItem.ParseLoaderOrder(items, out var primaryLoader, out var _);
        string loader = primaryLoader!.Type.ToString().ToLower();

        if (ModrinthProject.Loaders.Contains(loader))
        {
            Conflicted = false;

            if (Files != null)
            {
                FilteredFiles = [.. Files.Where(f => f.Loaders.Contains(loader))];
                SelectedModrinthFile = FilteredFiles.FirstOrDefault();
            }

            return;
        }

        Conflicted = true;
        IsSelected = false;
    }

    public bool GetTextLoad(string text) => !string.IsNullOrWhiteSpace(text);

    public static async Task<InstanceModItem[]> GetInstanceModItemsAsync(VersionManifestItem manifestItem)
    {
        var curseForgeClient = App.GetService<CurseForgeClient>();
        var modrinthClient = App.GetService<ModrinthClient>();
        var items = new List<InstanceModItem>();

        string[] modrinthIds = 
        [
            "P7dR8mSH", // Fabric Api
            "qvIfYCYJ", // QSL
            "AANobbMI", // Sodium
            "YL57xq9U", // Iris
        ];

        foreach (var id in modrinthIds)
        {
            ModrinthProject modrinthProject;

            try { modrinthProject = await modrinthClient.GetProjectFromId(id); }
            catch (Exception) { continue; }

            InstanceModItem instanceMod = new() { ModrinthProject = modrinthProject };
            items.Add(instanceMod);

            Task.Run(async () =>
            {
                ModrinthFile[]? modrinthFiles = null;

                try
                {
                    modrinthFiles = [.. (await modrinthClient.GetProjectVersionsAsync(id)).Where(f => f.McVersion.Equals(manifestItem.Id))];
                }
                finally
                {
                    await App.DispatcherQueue.EnqueueAsync(() =>
                    {
                        WeakReferenceMessenger.Default.Send(new InstanceLoaderQueryMessage());

                        instanceMod.Files = modrinthFiles;
                        instanceMod.FilteredFiles = modrinthFiles;
                        instanceMod.Supported = modrinthFiles != null && modrinthFiles.Length != 0;
                        instanceMod.Loading = false;

                        if (modrinthFiles?.Length > 0)
                            instanceMod.SelectedModrinthFile = modrinthFiles[0];
                    });
                }
            }).Forget();
        }

        return [.. items];
    }
}

internal class InstanceInstallConfig
{
    public required string InstanceId { get; set; }

    public required VersionManifestItem ManifestItem { get; set; }

    public bool EnableIndependencyInstance { get; set; }

    public InstanceLoaderItem? PrimaryLoader { get; set; }

    public InstanceLoaderItem? SecondaryLoader { get; set; }

    public List<ModrinthFile> AdditionalMods { get; set; } = [];
}

internal class ModpackInstallConfig
{
    public required string ModpackFilePath { get; set; }

    public required string InstanceId { get; set; }

    public required ModpackInfo ModpackInfo { get; set; }
}