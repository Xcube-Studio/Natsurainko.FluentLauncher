using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement.Installer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        ModLoaderType.NeoForge => [ModLoaderType.Forge, ModLoaderType.Fabric, ModLoaderType.Quilt],
        ModLoaderType.Fabric => [ModLoaderType.NeoForge, ModLoaderType.Forge, ModLoaderType.Quilt],
        ModLoaderType.Quilt => [ModLoaderType.NeoForge, ModLoaderType.Fabric, ModLoaderType.Forge, ModLoaderType.OptiFine],
        ModLoaderType.OptiFine => [ModLoaderType.Quilt],
        _ => []
    };

    public bool GetTextLoad(string text) => !string.IsNullOrWhiteSpace(text);

    public static InstanceLoaderItem[] GetInstanceLoaderItems(VersionManifestItem manifestItem)
    {
        var cacheInterfaceService = App.GetService<CacheInterfaceService>();
        var items = new List<InstanceLoaderItem>();

        foreach (ModLoaderType type in new ModLoaderType[] { ModLoaderType.NeoForge, ModLoaderType.Forge, ModLoaderType.OptiFine, ModLoaderType.Fabric, ModLoaderType.Quilt })
        {
            var item = new InstanceLoaderItem { Type = type };
            items.Add(item);

            Task.Run(async () => 
            {
                object[] installDatas = [];

                try
                {
                    string requestUrl = type switch
                    {
                        ModLoaderType.NeoForge => $"https://bmclapi2.bangbang93.com/neoforge/list/{manifestItem.Id}",
                        ModLoaderType.Forge => $"https://bmclapi2.bangbang93.com/forge/minecraft/{manifestItem.Id}",
                        ModLoaderType.OptiFine => $"https://bmclapi2.bangbang93.com/optifine/{manifestItem.Id}",
                        ModLoaderType.Fabric => $"https://meta.fabricmc.net/v2/versions/loader/{manifestItem.Id}",
                        ModLoaderType.Quilt  => $"https://meta.quiltmc.org/v3/versions/loader/{manifestItem.Id}"
                    };

                    string jsonContent = (await cacheInterfaceService.RequestStringAsync(requestUrl, Services.Network.Data.InterfaceRequestMethod.AlwaysLatest))!;

                    installDatas = type switch
                    {
                        ModLoaderType.NeoForge => [.. JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.ForgeInstallDataArray)!.OrderByDescending(x => x, new ForgeVersionComparer())!],
                        ModLoaderType.Forge => [.. JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.ForgeInstallDataArray)!.OrderByDescending(x => x, new ForgeVersionComparer())!],
                        ModLoaderType.OptiFine => [.. JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.OptiFineInstallDataArray)!.OrderByDescending(x => x, new OptiFineVersionComparer())!],
                        ModLoaderType.Fabric => JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.FabricInstallDataArray)!,
                        ModLoaderType.Quilt => JsonSerializer.Deserialize(jsonContent, FLSerializerContext.Default.QuiltInstallDataArray)!,
                        _ => throw new InvalidOperationException()
                    };
                }
                catch (Exception) { }

                await App.DispatcherQueue.EnqueueAsync(() =>
                {
                    item.InstallDatas = installDatas;
                    item.Supported = installDatas.Length != 0;
                    item.Loading = false;

                    if (installDatas.Length > 0) 
                        item.SelectedInstallData = installDatas[0];
                });
            });
        }

        return [.. items]; 
    }
}
