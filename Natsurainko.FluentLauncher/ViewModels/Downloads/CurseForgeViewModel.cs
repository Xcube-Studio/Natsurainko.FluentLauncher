using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services;
using Natsurainko.FluentLauncher.Services.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CurseForgeViewModel : ObservableObject
{
    public CurseForgeViewModel(CurseForgeModService curseForgeModService)
    {
        ModService = curseForgeModService;

        var tasks = new Task[]
        {
            curseForgeModService.GetCurseForgeCategoriesAsync(),
            curseForgeModService.GetVersionsAsync(),
            curseForgeModService.GetFeaturedResourcesAsync()
        };

        Task.WhenAll(tasks).ContinueWith(task =>
        {
            var curseForgeCategories = (tasks[0] as Task<CurseForgeCategoryData[]>).Result;
            var curseForgeResources = (tasks[2] as Task<CurseForgeResourceData[]>).Result;
            var versions = (tasks[1] as Task<string[]>).Result;

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                Categories = curseForgeCategories;
                Resources = curseForgeResources;
                Versions = versions;
                SelectedVersion = versions[0];
            });
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
                ;
        });
    }

    private CurseForgeModService ModService { get; }

    [ObservableProperty]
    private IReadOnlyList<CurseForgeResourceData> resources;

    [ObservableProperty]
    private IReadOnlyList<CurseForgeCategoryData> categories;

    [ObservableProperty]
    private IReadOnlyList<string> versions;

    [ObservableProperty]
    private CurseForgeCategoryData selectedCategory;

    [ObservableProperty]
    private string selectedVersion;

    [ObservableProperty]
    private bool enableCategory;

    [RelayCommand]
    public Task Search(string name) => Task.Run(async () =>
    {
        var resources = (await ModService.SearchResourcesAsync(
            name,
            gameVersion: selectedVersion.Equals("All") ? default : selectedVersion,
            categoryId: enableCategory ? selectedCategory.Id : default));

        App.MainWindow.DispatcherQueue.TryEnqueue(() => Resources = resources);
    });
}
