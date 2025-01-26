using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService { get; init; }

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
            NavigationService.NavigateTo(pageKey);
        else
            NavigationService.NavigateTo("Settings/Default"); // Default page
    }

    public void HandleNavigationBreadcrumBarItemClicked(object args)
    {
        var routes = (string[])args;

        if (routes.Length >=1 && routes[0] == "Settings")
            NavigationService.NavigateTo("Settings/Default");
        else
            NavigationService.NavigateTo(string.Join('/', routes.ToArray()));
    }
}
