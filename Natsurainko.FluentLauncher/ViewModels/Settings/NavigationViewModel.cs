using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService { get; init; }

    public ObservableCollection<string> DisplayedPath { get; } = new();

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
        {
            NavigationService.NavigateTo(pageKey);
            foreach (string item in pageKey.Split('/'))
            {
                DisplayedPath.Add(item);
            }
        }
        else
        {
            NavigationService.NavigateTo("Settings/Default"); // Default page
            DisplayedPath.Add("Settings");
        }
    }

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length >=1 && routes[0] == "Settings")
        {
            NavigationService.NavigateTo("Settings/Default"); // Default page
            DisplayedPath.Add("Settings");
        }
        else
        {
            NavigationService.NavigateTo(string.Join('/', routes));
            foreach (string item in routes)
            {
                DisplayedPath.Add(item);
            }
        }
    }
}
