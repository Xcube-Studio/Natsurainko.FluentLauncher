using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<string> routes;

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
            _navigationService.NavigateTo("Tasks/Default"); // Default page
        }
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        _navigationService.NavigateTo(pageKey, parameter);
        Routes = new(pageKey == "Tasks/Default" ? ["Tasks"] : pageKey.Split('/'));
    }

    [RelayCommand]
    public void ItemClickedEvent(object args)
    {
        var breadcrumbBarItemClickedEventArgs = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>().args;

        if (breadcrumbBarItemClickedEventArgs.Item.ToString() == "Tasks")
            NavigateTo("Tasks/Default");
        else NavigateTo(string.Join('/', Routes.ToArray()[..^1]));
    }
}
