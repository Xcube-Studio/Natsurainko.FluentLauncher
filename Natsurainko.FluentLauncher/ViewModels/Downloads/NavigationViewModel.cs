using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Data;
using Natsurainko.FluentLauncher.Utils;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    private readonly SearchProviderService _searchProviderService;

    public INavigationService NavigationService { get; init; }

    [ObservableProperty]
    public partial ObservableCollection<string> Routes { get; set; }

    public NavigationViewModel(INavigationService navigationService, SearchProviderService searchProviderService)
    {
        NavigationService = navigationService;
        _searchProviderService = searchProviderService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is SearchOptions)
        {
            Routes = new("Download/Search".Split('/'));
            NavigateTo("Download/Search", parameter);
        }
        else if (parameter is string pageKey)
        {
            Routes = new(pageKey.Split('/'));
            NavigateTo(pageKey);
        }
        else
        {
            Routes = [];
            NavigationService.NavigateTo("Download/Default"); // Default page
        }
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter);
        Routes = new(pageKey == "Download/Default" ? ["Download"] : pageKey.Split('/'));
    }

    void QueryReceiver(string searchText) => NavigateTo("Download/Search", new SearchOptions { SearchText = searchText });

    [RelayCommand]
    void Loaded()
    {
        if (_searchProviderService.QueryReceiverOwner != this)
            _searchProviderService.OccupyQueryReceiver(this, QueryReceiver);
    }

    [RelayCommand]
    void ItemClickedEvent(object args)
    {
        var breadcrumbBarItemClickedEventArgs = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>().args;

        if (breadcrumbBarItemClickedEventArgs.Item.ToString() == "Download")
            NavigateTo("Download/Default");
        else NavigateTo(string.Join('/', Routes.ToArray()[..^1]));
    }
}
