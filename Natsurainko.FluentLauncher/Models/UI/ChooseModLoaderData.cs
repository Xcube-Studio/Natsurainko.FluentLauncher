using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Experimental.GameManagement.ModLoaders;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Models.UI;

internal partial class ChooseModLoaderData : ObservableObject
{
    [ObservableProperty]
    private ModLoaderType type;

    [ObservableProperty]
    private bool isChecked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnable))]
    private bool isSupported = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEnable))]
    private bool isCompatible = true;

    [ObservableProperty]
    private bool isLoading = true;

    public bool IsEnable => IsSupported && IsCompatible;

    public readonly VersionManifestItem _manifestItem;
    private readonly List<ChooseModLoaderData> _chooseModLoaders;

    public ChooseModLoaderData(ModLoaderType type, VersionManifestItem manifestItem, List<ChooseModLoaderData> chooseModLoaders)
    {
        Type = type;
        Description = ResourceUtils.GetValue("CoreInstallWizard", "ChooseModLoaderPage", $"_{Type}");

        _manifestItem = manifestItem;
        _chooseModLoaders = chooseModLoaders;

        Init();
    }

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private string displayText = ResourceUtils.GetValue("CoreInstallWizard", "ChooseModLoaderPage", "_Loading");

    [ObservableProperty]
    private IEnumerable<LoaderBuildData>? items;

    [ObservableProperty]
    private LoaderBuildData? selectedItem;

    private void Init() => Task.Run(() =>
    {
        var url = Type switch
        {
            ModLoaderType.NeoForge => $"https://bmclapi2.bangbang93.com/neoforge/list/{_manifestItem.Id}",
            ModLoaderType.Forge => $"https://bmclapi2.bangbang93.com/forge/minecraft/{_manifestItem.Id}",
            ModLoaderType.OptiFine => $"https://bmclapi2.bangbang93.com/optifine/{_manifestItem.Id}",
            ModLoaderType.Fabric => $"https://meta.fabricmc.net/v2/versions/loader/{_manifestItem.Id}",
            ModLoaderType.Quilt => $"https://meta.quiltmc.org/v3/versions/loader/{_manifestItem.Id}",
            _ => throw new NotImplementedException()
        };

        using var responseMessage = HttpUtils.HttpGet(url);
        responseMessage.EnsureSuccessStatusCode();

        var array = responseMessage.Content.ReadAsString()
            .ToJsonNode()?
            .AsArray()
            .WhereNotNull();
        IEnumerable<LoaderBuildData>? loaders = null;

        if (array is not null && array.Any())
        {
            switch (Type)
            {
                case ModLoaderType.Forge:

                    var forge = array.Select(x =>
                    {
                        var displayText = x["version"]?.GetValue<string>();
                        var metadata = x["build"];
                        if (displayText is null || metadata is null)
                            throw new Exception("Invalid forge data");

                        return new LoaderBuildData
                        {
                            DisplayText = displayText,
                            Metadata = metadata
                        };
                    }).WhereNotNull().ToList();

                    forge.Sort((a, b) => a.Metadata.GetValue<int>().CompareTo(b.Metadata.GetValue<int>()));
                    forge.Reverse();

                    loaders = forge;

                    break;
                case ModLoaderType.Fabric:

                    var fabric = array.Select(x =>
                    {
                        var displayText = x["loader"]?["version"]?.GetValue<string>();
                        if (displayText is null)
                            throw new Exception("Invalid fabric data");

                        return new LoaderBuildData
                        {
                            DisplayText = displayText,
                            Metadata = x
                        };
                    }).ToList();

                    loaders = fabric;

                    break;
                case ModLoaderType.OptiFine:

                    var optifine = array.Select(x =>
                    {
                        var type = x["type"]?.GetValue<string>();
                        var patch = x["patch"]?.GetValue<string>();
                        if (type is null || patch is null)
                            throw new Exception("Invalid optifine data");

                        return new LoaderBuildData
                        {
                            DisplayText = $"{type}_{patch}",
                            Metadata = x
                        };
                    }).ToList();

                    loaders = optifine;

                    break;
                case ModLoaderType.Quilt:

                    var quilt = array.Select(x => new LoaderBuildData
                    {
                        DisplayText = x!["loader"]!["version"]!.GetValue<string>(),
                        Metadata = x
                    }).ToList();

                    loaders = quilt;

                    break;
                case ModLoaderType.NeoForge:

                    var neoForge = array.Select(x => new LoaderBuildData
                    {
                        DisplayText = x["version"]!.GetValue<string>(),
                        Metadata = x["version"]!
                    }).ToList();

                    neoForge.Sort((a, b) => a.DisplayText.CompareTo(b.DisplayText));
                    neoForge.Reverse();

                    loaders = neoForge;

                    break;
            }
        }

        App.DispatcherQueue.TryEnqueue(() =>
        {
            IsLoading = false;

            if (array!.Any())
            {
                Items = loaders;
                SelectedItem = loaders!.First();
                IsSupported = true;
            }

            if (!IsSupported) DisplayText = ResourceUtils.GetValue("CoreInstallWizard", "ChooseModLoaderPage", "_NotSupported"); ;
        });
    });

    public void HandleCompatible()
    {
        bool compatible = true;
        var uncompatibleModLoaderTypes = new List<ModLoaderType>();

        switch (Type)
        {
            case ModLoaderType.Forge:
                uncompatibleModLoaderTypes.Add(ModLoaderType.NeoForge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Fabric);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
            case ModLoaderType.Fabric:
                uncompatibleModLoaderTypes.Add(ModLoaderType.NeoForge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Forge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
            case ModLoaderType.Quilt:
                uncompatibleModLoaderTypes.Add(ModLoaderType.NeoForge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Forge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Fabric);
                uncompatibleModLoaderTypes.Add(ModLoaderType.OptiFine);
                break;
            case ModLoaderType.NeoForge:
                uncompatibleModLoaderTypes.Add(ModLoaderType.Forge);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Fabric);
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
            case ModLoaderType.OptiFine:
                uncompatibleModLoaderTypes.Add(ModLoaderType.Quilt);
                break;
        }

        foreach (var data in _chooseModLoaders.Where(x => x.IsChecked))
            if (uncompatibleModLoaderTypes.Contains(data.Type))
            {
                compatible = false;
                break;
            }

        IsCompatible = compatible;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(IsChecked))
            _chooseModLoaders.ForEach(x => x.HandleCompatible());
    }

    public class LoaderBuildData
    {
        public required string DisplayText { get; set; }

        public required JsonNode Metadata { get; set; }
    }
}
