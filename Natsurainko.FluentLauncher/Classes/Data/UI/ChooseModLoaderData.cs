using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Classes.Enums;
using Nrk.FluentCore.GameResources.ThirdPartySources;
using Nrk.FluentCore.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Classes.Data.UI;

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
    private IEnumerable<LoaderBuildData> items;

    [ObservableProperty]
    private LoaderBuildData selectedItem;

    private void Init() => Task.Run(() =>
    {
        var url = Type switch
        {
            ModLoaderType.NeoForge => $"{DownloadMirrors.Bmclapi.Domain}/neoforge/list/{_manifestItem.Id}",
            ModLoaderType.Forge => $"{DownloadMirrors.Bmclapi.Domain}/forge/minecraft/{_manifestItem.Id}",
            ModLoaderType.OptiFine => $"{DownloadMirrors.Bmclapi.Domain}/optifine/{_manifestItem.Id}",
            ModLoaderType.Fabric => $"https://meta.fabricmc.net/v2/versions/loader/{_manifestItem.Id}",
            ModLoaderType.Quilt => $"https://meta.quiltmc.org/v3/versions/loader/{_manifestItem.Id}"
        };

        using var responseMessage = HttpUtils.HttpGet(url);
        responseMessage.EnsureSuccessStatusCode();

        var array = JsonNode.Parse(responseMessage.Content.ReadAsString()).AsArray();
        IEnumerable<LoaderBuildData> loaders = default;

        if (array.Any())
        {
            switch (Type)
            {
                case ModLoaderType.Forge:

                    var forge = array.Select(x => new LoaderBuildData
                    {
                        DisplayText = x["version"].GetValue<string>(),
                        Metadata = x["build"]
                    }).ToList();

                    forge.Sort((a, b) => a.Metadata.GetValue<int>().CompareTo(b.Metadata.GetValue<int>()));
                    forge.Reverse();

                    loaders = forge;

                    break;
                case ModLoaderType.Fabric:

                    var fabric = array.Select(x => new LoaderBuildData
                    {
                        DisplayText = x["loader"]["version"].GetValue<string>(),
                        Metadata = x
                    }).ToList(); 

                    loaders = fabric;

                    break;
                case ModLoaderType.OptiFine:

                    var optifine = array.Select(x => new LoaderBuildData
                    {
                        DisplayText = $"{x["type"].GetValue<string>()}_{x["patch"].GetValue<string>()}",
                        Metadata = x
                    }).ToList();

                    loaders = optifine;

                    break;
                case ModLoaderType.Quilt:

                    var quilt = array.Select(x => new LoaderBuildData
                    {
                        DisplayText = x["loader"]["version"].GetValue<string>(),
                        Metadata = x
                    }).ToList();

                    loaders = quilt;

                    break;
                case ModLoaderType.NeoForge:

                    var neoForge = array.Select(x => new LoaderBuildData
                    {
                        DisplayText = x["version"].GetValue<string>(),
                        Metadata = x["version"]
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

            if (array.Any())
            {
                Items = loaders;
                SelectedItem = loaders.First();
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
        public string DisplayText { get; set; }

        public JsonNode Metadata { get; set; }
    }
}
