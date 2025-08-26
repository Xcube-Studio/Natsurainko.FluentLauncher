using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal abstract partial class ResourceDefaultViewModel(
    CurseForgeClient curseForgeClient,
    ModrinthClient modrinthClient,
    SettingsService settingsService,
    INavigationService navigationService,
    SearchProviderService searchProviderService) : PageVM, INavigationAware
{
    private string? _pageKey;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool depressCategoryChangedInvokeSearch = true;

    [ObservableProperty]
    public partial ObservableCollection<object>? SearchResult { get; set; }

    [ObservableProperty]
    public partial int ResourceSource { get; set; } = 0;

    [ObservableProperty]
    public partial string[] Categories { get; set; } = null!;

    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FilteredVersion { get; set; }

    [ObservableProperty]
    public partial bool Loading { get; set; }

    [ObservableProperty]
    public partial bool LoadFailed { get; set; }

    [ObservableProperty]
    public partial bool Searched { get; set; }

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    protected abstract CurseForgeResourceType CurseForgeResourceType { get; }

    protected abstract ModrinthResourceType ModrinthResourceType { get; }

    protected abstract string[] ModrinthCategories { get; }

    protected abstract Dictionary<string, int> CurseForgeCategories { get; }

    partial void OnResourceSourceChanged(int value)
    {
        depressCategoryChangedInvokeSearch = true;

        UpdateFiltersSource();
        SearchReceiveHandle(SearchQuery);

        depressCategoryChangedInvokeSearch = false;
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        if (!depressCategoryChangedInvokeSearch)
            SearchReceiveHandle(SearchQuery);
    }

    partial void OnFilteredVersionChanged(string value) => SearchReceiveHandle(SearchQuery);

    void INavigationAware.SetNavigationKey(string key) => _pageKey = key;

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        UpdateFiltersSource();

        if (parameter is string query)
            SearchQuery = query;
    }

    [RelayCommand]
    void ResourceItemInvoke(object mod) => navigationService.NavigateTo(_pageKey!.Replace("Default", "Resource"), mod);

    [RelayCommand]
    void ClearSearchQuery()
    {
        searchProviderService.ClearSearchBox();
        SearchReceiveHandle(string.Empty);
    }

    void SearchReceiveHandle(string query, bool slugMode = false)
    {
        if (!IsActive) return;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new();

        Task.Run(async () =>
        {
            await Dispatcher.EnqueueAsync(() =>
            {
                SearchResult = [];
                Loading = true;
            });

            object[]? result = null;

            try
            {
                result = ResourceSource switch
                {
                    0 => (await curseForgeClient.SearchResourcesAsync(query, CurseForgeResourceType,
                            categoryId: CurseForgeCategories[SelectedCategory],
                            version: FilteredVersion == LocalizedStrings.ResourceCategories__All ? null : FilteredVersion,
                            slugMode: slugMode,
                            cancellationToken: _cancellationTokenSource.Token)).ToArray(),
                    1 => (await modrinthClient.SearchResourcesAsync(query, ModrinthResourceType,
                            categories: SelectedCategory == "all" ? null : SelectedCategory,
                            version: FilteredVersion == LocalizedStrings.ResourceCategories__All ? null : FilteredVersion,
                            cancellationToken: _cancellationTokenSource.Token)).ToArray(),
                    _ => null
                };
            }
            catch
            {
                await Dispatcher.EnqueueAsync(() => LoadFailed = true);
            }
            finally
            {
                await Dispatcher.EnqueueAsync(() =>
                {
                    Loading = false;
                    SearchResult = result == null ? null : new(result);
                    Searched = !string.IsNullOrEmpty(query);
                    SearchQuery = query;

                    if (slugMode && ResourceSource == 0 && result?.Length == 1)
                        ResourceItemInvoke(result.First());
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

    protected override void OnLoaded()
    {
        searchProviderService.OccupyQueryReceiver(this, query => SearchReceiveHandle(query));
        searchProviderService.RegisterSuggestionProvider(this, SuggestionProvider);

        SearchReceiveHandle(SearchQuery);

        depressCategoryChangedInvokeSearch = false;
    }

    protected override void OnUnloaded()
    {
        searchProviderService.UnregisterSuggestionProvider(this);

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();

        GC.Collect();
    }

    private IEnumerable<Suggestion> SuggestionProvider(string query)
    {
        if (!settingsService.EnableLocalizedResourceSuggestions)
            return [];

        if (CurseForgeResourceType != CurseForgeResourceType.McMod || string.IsNullOrEmpty(query))
            return [];

        return SuggestionHelper.GetSearchModSuggestions(query, ResourceSource, slug => SearchReceiveHandle(slug, true));
    }

    public static readonly string[] MinecraftVersions = [
        LocalizedStrings.ResourceCategories__All,
        "1.21.9", "1.21.8", "1.21.7", "1.21.6", "1.21.5", "1.21.4", "1.21.3", "1.21.2", "1.21.1", "1.21",
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

