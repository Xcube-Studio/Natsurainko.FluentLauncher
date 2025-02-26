using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

internal partial class DefaultViewModel : ObservableRecipient, INavigationAware
{
    private readonly CurseForgeClient _curseForgeClient;
    private readonly ModrinthClient _modrinthClient;
    private readonly INavigationService _navigationService;
    private readonly SearchProviderService _searchProviderService;

    private CancellationTokenSource? _cancellationTokenSource;

    public DefaultViewModel(
        CurseForgeClient curseForgeClient, 
        ModrinthClient modrinthClient,
        INavigationService navigationService,
        SearchProviderService searchProviderService)
    {
        _curseForgeClient = curseForgeClient;
        _modrinthClient = modrinthClient;
        _navigationService = navigationService;
        _searchProviderService = searchProviderService;

        IsActive = true;
    }

    [ObservableProperty]
    public partial List<object>? SearchResult { get; set; }

    [ObservableProperty]
    public partial int ResourceSource { get; set; } = 0;

    [ObservableProperty]
    public partial string[] Categories { get; set; } = null!;

    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilteredVersion { get; set; } = LocalizedStrings.Downloads_Mods_DefaultPage__All;

    [ObservableProperty]
    public partial bool Loading { get; set; }

    [ObservableProperty]
    public partial bool LoadFailed { get; set; }

    [ObservableProperty]
    public partial bool Searched { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    public string[] Versions => MinecraftVersions;

    partial void OnResourceSourceChanged(int value)
    {
        UpdateFiltersSource();
        SearchReceiveHandle(SearchQuery);
    }

    partial void OnSelectedCategoryChanged(string value) => SearchReceiveHandle(SearchQuery);

    partial void OnFilteredVersionChanged(string value) => SearchReceiveHandle(SearchQuery);

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        UpdateFiltersSource();

        if (parameter is string query)
            SearchReceiveHandle(query);
        else
            SearchReceiveHandle(SearchQuery);

        _searchProviderService.OccupyQueryReceiver(this, SearchReceiveHandle);
    }

    [RelayCommand]
    void CardClick(object mod) => _navigationService.NavigateTo("ModsDownload/Mod", mod);

    [RelayCommand]
    void ClearSearchQuery()
    {
        _searchProviderService.ClearSearchBox();
        SearchReceiveHandle(string.Empty);
    }

    void SearchReceiveHandle(string query)
    {
        if (!IsActive) return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        Task.Run(async () =>
        {
            await App.DispatcherQueue.EnqueueAsync(() => Loading = true);
            IEnumerable<object> result = [];

            try
            {
                result = ResourceSource == 0
                    ? await _curseForgeClient.SearchResourcesAsync(
                        query, 
                        CurseForgeResourceType.McMod, 
                        categoryId: CurseForgeCategories[SelectedCategory],
                        version: FilteredVersion == LocalizedStrings.Downloads_Mods_DefaultPage__All ? null : FilteredVersion,
                        cancellationToken: _cancellationTokenSource.Token)
                    : await _modrinthClient.SearchResourcesAsync(
                        query, 
                        ModrinthResourceType.McMod,
                        categories: SelectedCategory == "all" ? null : SelectedCategory,
                        version: FilteredVersion == LocalizedStrings.Downloads_Mods_DefaultPage__All ? null : FilteredVersion,
                        cancellationToken: _cancellationTokenSource.Token);

                if (_cancellationTokenSource.IsCancellationRequested)
                    result = [];
            }
            catch
            {
                await App.DispatcherQueue.EnqueueAsync(() => LoadFailed = true);
            }
            finally
            {
                await App.DispatcherQueue.EnqueueAsync(() =>
                {
                    Loading = false;
                    SearchResult = [.. result];
                    Searched = !string.IsNullOrEmpty(query);
                    SearchQuery = query;
                });
            }
        }, _cancellationTokenSource.Token);
    }

    void UpdateFiltersSource()
    {
        Categories = ResourceSource == 0 
            ? [.. CurseForgeCategories.Keys]
            : ModrinthCategories;

        if (!Categories.Contains(SelectedCategory))
            SelectedCategory = Categories[0];
    }

    static readonly string[] ModrinthCategories = [
        "all", "adventure", "cursed", "decoration", "economy", "equipment", "food",
        "game-mechanics", "library", "magic", "management", "minigame", "mobs", "optimization",
        "social", "storage", "technology", "transportation", "utility", "worldgen"
    ];

    static readonly Dictionary<string, int> CurseForgeCategories = new()
    {
        { "All", 0 }, { "Map and Information", 423 }, { "Armor, Tools, and Weapons", 434 }, { "API and Library", 421 }, { "Adventure and RPG", 422 }, { "Processing", 413 },
        { "Utility & QoL", 5191 }, { "Education", 5299 }, { "Miscellaneous", 425 }, { "Server Utility", 435 }, { "Technology", 412 }, { "Food", 436 }, { "World Gen", 406 },
        { "Storage", 420 }, { "Structures", 409 }, { "Addons", 426 }, { "Tinker's Construct", 428 }, { "Blood Magic", 4485 }, { "Bug Fixes", 6821 }, { "Industrial Craft", 429 },
        { "Galacticraft", 5232 }, { "Farming", 416 }, { "Magic", 419 }, { "Automation", 4843 }, { "Applied Energistics 2", 4545 }, { "Twitch Integration", 4671 },
        { "CraftTweaker", 4773 }, { "Integrated Dynamics", 6954 }, { "Create", 6484 }, { "Mobs", 411 }, { "Skyblock", 6145 }, { "MCreator", 4906 }, { "Cosmetic", 424 },
        { "KubeJS", 5314 }, { "Redstone", 4558 }, { "Performance", 6814 }, { "Player Transport", 414 }, { "Biomes", 407 }, { "Ores and Resources", 408 },
        { "Energy, Fluid, and Item Transport", 415 }, { "Buildcraft", 432 }, { "Thaumcraft", 430 }, { "Thermal Expansion", 427 }, { "Dimensions", 410 }, { "Energy", 417 },
        { "Twilight Forest", 7669 }, { "Genetics", 418 }, { "Forestry", 433 }
    };

    static readonly string[] MinecraftVersions = [
        LocalizedStrings.Downloads_Mods_DefaultPage__All,
        "1.21.4", "1.21.3", "1.21.2", "1.21.1", "1.21",
        "1.20.6", "1.20.5", "1.20.4", "1.20.3", "1.20.2", "1.20.1", "1.20",
        "1.19.4", "1.19.3", "1.19.2", "1.19.1", "1.19",
        "1.18.2", "1.18.1", "1.18",
        "1.17.1", "1.17",
        "1.16.5", "1.16.4", "1.16.3", "1.16.2", "1.16.1", "1.16",
        "1.15.2", "1.15.1", "1.15",
        "1.14.4", "1.14.3", "1.14.2", "1.14.1", "1.14",
        "1.13.2", "1.13.1", "1.13",
        "1.12.2", "1.12.1", "1.12", 
        "1.11.2", "1.11.1", "1.11",
        "1.10.2", "1.10.1", "1.10",
        "1.9.4", "1.9.3", "1.9.2", "1.9.1", "1.9",
        "1.8.9", "1.8.8", "1.8.7", "1.8.6", "1.8.5", "1.8.4", "1.8.3", "1.8.2", "1.8.1", "1.8",
        "1.7.10", "1.7.9", "1.7.8", "1.7.7", "1.7.6", "1.7.5", "1.7.4", "1.7.3", "1.7.2",
        "1.6.4", "1.6.2", "1.6.1",
        "1.5.2", "1.5.1",
        "1.4.7", "1.4.6", "1.4.5", "1.4.4", "1.4.2",
        "1.3.2", "1.3.1",
        "1.2.5", "1.2.4", "1.2.3", "1.2.2", "1.2.1",
        "1.1", "1.0",
    ];
}
