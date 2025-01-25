using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService { get; init; }

    [ObservableProperty]
    public partial ObservableCollection<string> Routes { get; set; }

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
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
            NavigationService.NavigateTo("Settings/Default"); // Default page
        }
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter);
        Routes = new(pageKey == "Settings/Default" ? ["Settings"] : pageKey.Split('/'));
    }

    [RelayCommand]
    public void ItemClickedEvent(object args)
    {
        var breadcrumbBarItemClickedEventArgs = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>().args;

        if (breadcrumbBarItemClickedEventArgs.Item.ToString() == "Settings")
            NavigateTo("Settings/Default");
        else NavigateTo(string.Join('/', Routes.ToArray()[..^1]));
    }
}
