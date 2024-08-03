using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Network;
using Nrk.FluentCore.Management.Downloader.Data;
using Nrk.FluentCore.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly CacheInterfaceService _cacheInterfaceService;

    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;

    public DefaultViewModel(
        INavigationService navigationService, 
        CacheInterfaceService cacheInterfaceService,
        CurseForgeClient curseForgeClient,
        ModrinthClient modrinthClient)
    {
        _navigationService = navigationService;
        _cacheInterfaceService = cacheInterfaceService;
        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
    }

    [ObservableProperty]
    private PatchNoteData[] patchNoteDatas;

    [ObservableProperty]
    private VersionManifestItem[] versionManifestItems;

    [ObservableProperty]
    private CurseForgeResource[] curseForgeResources;

    [ObservableProperty]
    private ModrinthResource[] modrinthResources;

    private string PatchNotesJson;
    private string VersionManifestJson;

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _cacheInterfaceService.RequestStringAsync(
            CacheInterfaceService.LauncherContentPatchNotes,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParsePatchNotesTask,
            "cache-interfaces\\launchercontent.mojang.com\\javaPatchNotes.json")
        .ContinueWith(ParsePatchNotesTask);


        _cacheInterfaceService.RequestStringAsync(
            _cacheInterfaceService.VersionManifest,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParseVersionManifestTask,
            "cache-interfaces\\piston-meta.mojang.com\\version_manifest_v2.json")
        .ContinueWith(ParseVersionManifestTask);

        _curseForgeClient.SearchResourcesAsync(string.Empty).ContinueWith(ParseCurseForgeTask);
        _modrinthClient.SearchResourcesAsync(string.Empty).ContinueWith(ParseModrinthTask);
    } 

    void ParsePatchNotesTask(Task<string> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        string patchNotesJson = task.Result;
        if (string.IsNullOrEmpty(patchNotesJson) || PatchNotesJson == patchNotesJson) 
            return;

        var patchNotes = JsonNode.Parse(patchNotesJson)!["entries"].AsArray().Select(node =>
        {
            var patchNote = node.Deserialize<PatchNoteData>();
            patchNote.ImageUrl = $"https://launchercontent.mojang.com{node["image"]!["url"].GetValue<string>()}";

            return patchNote;
        }).ToArray();

        var snapshot = patchNotes.Where(note => note.Type.Equals("snapshot")).Take(3);
        var release = patchNotes.Where(note => note.Type.Equals("release")).Take(2);

        var final = snapshot.Union(release).ToArray();

        PatchNotesJson = patchNotesJson;
        App.DispatcherQueue.TryEnqueue(() => PatchNoteDatas = final);
    }

    void ParseVersionManifestTask(Task<string> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        string versionManifestJson = task.Result;
        if (string.IsNullOrEmpty(versionManifestJson) || VersionManifestJson == versionManifestJson)
            return;

        var manifestItems = JsonNode.Parse(versionManifestJson)
            .Deserialize<VersionManifestJsonEntity>().Versions.Take(3).ToArray();

        App.DispatcherQueue.TryEnqueue(() => VersionManifestItems = manifestItems);

        VersionManifestJson = versionManifestJson;
    }

    void ParseCurseForgeTask(Task<IEnumerable<CurseForgeResource>> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        var curseForgeResources = task.Result.Reverse().Take(6).ToArray();
        App.DispatcherQueue.TryEnqueue(() => CurseForgeResources = curseForgeResources);
    }

    void ParseModrinthTask(Task<IEnumerable<ModrinthResource>> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        var modrinthResources = task.Result.ToArray();
        App.DispatcherQueue.TryEnqueue(() => ModrinthResources = modrinthResources);
    }
}
