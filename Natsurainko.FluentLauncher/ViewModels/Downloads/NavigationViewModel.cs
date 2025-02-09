using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Data;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    private readonly SearchProviderService _searchProviderService;

    public INavigationService NavigationService { get; init; }

    public ObservableCollection<string> DisplayedPath { get; } = new();

    public NavigationViewModel(INavigationService navigationService, SearchProviderService searchProviderService)
    {
        NavigationService = navigationService;
        _searchProviderService = searchProviderService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is SearchOptions)
        {
            NavigateTo("Download/Search", parameter);
        }
        else if (parameter is string pageKey)
        {
            NavigateTo(pageKey);
        }
        else
        {
            NavigateTo("Download/Default"); // Default page
        }
    }

    [RelayCommand]
    void Loaded()
    {
        if (_searchProviderService.QueryReceiverOwner == this) return;

        _searchProviderService.OccupyQueryReceiver(this, (searchText) =>
        {
            NavigationService.NavigateTo("Download/Search", new SearchOptions { SearchText = searchText });
        });
    }

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length >= 1 && routes[0] == "Download")
            NavigateTo("Download/Default");
        else
            NavigateTo(string.Join('/', routes));
    }

    private void NavigateTo(string pageKey, object? parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter); // Default page
        if (pageKey == "Download/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add("Download");
        }
        else
        {
            DisplayedPath.Clear();
            foreach (string item in pageKey.Split("/"))
            {
                DisplayedPath.Add(item);
            }
        }
    }
}
