using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.News;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    public partial ObservableCollection<string> Routes { get; set; }

    public NavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is string pageKey)
        {
            Routes = new(pageKey.Split('/'));
            NavigateTo(pageKey);
        }
        else
        {
            Routes = [];
            _navigationService.NavigateTo("News/Default"); // Default page
        }
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        _navigationService.NavigateTo(pageKey, parameter);
        Routes = new(pageKey == "News/Default" ? ["News"] : pageKey.Split('/'));
    }

    [RelayCommand]
    public void ItemClickedEvent(object args)
    {
        var breadcrumbBarItemClickedEventArgs = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>().args;

        if (breadcrumbBarItemClickedEventArgs.Item.ToString() == "News")
            NavigateTo("News/Default");
        else NavigateTo(string.Join('/', Routes.ToArray()[..^1]));
    }
}
