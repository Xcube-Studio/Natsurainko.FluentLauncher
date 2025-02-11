using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using System.Collections.ObjectModel;

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
            NavigateTo(pageKey);
        else
            NavigateTo("Settings/Default"); // Default page
    }

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length == 1 && routes[0] == "Settings")
            NavigateTo("Settings/Default");
        else
            NavigateTo(string.Join('/', routes));
    }

    private void NavigateTo(string pageKey)
    {
        NavigationService.NavigateTo(pageKey); // Default page
        if (pageKey == "Settings/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add("Settings");
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
