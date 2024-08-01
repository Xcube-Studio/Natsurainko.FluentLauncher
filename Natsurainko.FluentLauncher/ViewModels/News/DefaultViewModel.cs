using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Network;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.News;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly CacheInterfaceService _cacheInterfaceService;

    public DefaultViewModel(INavigationService navigationService, CacheInterfaceService cacheInterfaceService)
    {
        _navigationService = navigationService;
        _cacheInterfaceService = cacheInterfaceService;
    }

    [ObservableProperty]
    private PatchNoteData[] patchNoteDatas;

    [ObservableProperty]
    private NewsData[] newsDatas;

    private string PatchNotesJson;
    private string NewsJson;

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _cacheInterfaceService.RequestStringAsync(
            CacheInterfaceService.LauncherContentPatchNotes,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParsePatchNotesTask,
            "cache-interfaces\\launchercontent.mojang.com\\javaPatchNotes.json")
        .ContinueWith(ParsePatchNotesTask);

        _cacheInterfaceService.RequestStringAsync(
            CacheInterfaceService.LauncherContentNews,
            Services.Network.Data.InterfaceRequestMethod.PreferredLocal,
            ParseNewsTask,
            "cache-interfaces\\launchercontent.mojang.com\\news.json")
        .ContinueWith(ParseNewsTask);
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
        var release = patchNotes.Where(note => note.Type.Equals("release")).Take(3);

        var final = release.Union(snapshot).ToArray();

        PatchNotesJson = patchNotesJson;
        App.DispatcherQueue.TryEnqueue(() => PatchNoteDatas = final);
    }

    void ParseNewsTask(Task<string> task)
    {
        if (task.IsFaulted)
        {
            return;
        }

        string newsJson = task.Result;
        if (string.IsNullOrEmpty(newsJson) || NewsJson == newsJson) 
            return;

        var newsDatas = JsonNode.Parse(newsJson)!["entries"].AsArray().Select(node =>
        {
            var newsData = node.Deserialize<NewsData>();
            newsData.ImageUrl = $"https://launchercontent.mojang.com{node["newsPageImage"]!["url"].GetValue<string>()}";

            var tags = new List<string>([newsData.Date, newsData.Category]);

            if (newsData.Tags != null) 
                tags.Add(newsData.Tag);

            newsData.Tags = tags.ToArray();

            return newsData;
        }).Take(30).ToArray();

        NewsJson = newsJson;
        App.DispatcherQueue.TryEnqueue(() => NewsDatas = newsDatas);
    }

    [RelayCommand]
    public void ClickPatchNote(PatchNoteData patchNote)
    {
        _navigationService.NavigateTo("News/Note", patchNote);
    }
}
