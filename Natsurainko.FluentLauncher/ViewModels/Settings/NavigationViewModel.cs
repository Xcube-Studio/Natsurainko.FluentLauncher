using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using System.Collections.ObjectModel;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Settings;

public class NavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public ObservableCollection<string> Routes { get; set; }

    public NavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is string pageKey)
        {
            _navigationService.NavigateTo(pageKey);
            Routes = new(pageKey.Split('/'));
        }
        else
        {
            _navigationService.NavigateTo("Settings/Default"); // Default page
            Routes = [];
        }
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        _navigationService.NavigateTo(pageKey, parameter);
        Routes = new(pageKey.Split('/'));
    }
}
