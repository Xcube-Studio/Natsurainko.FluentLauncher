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
    public INavigationService NavigationService { get; init; }

    public ObservableCollection<string> DisplayedPath { get; } = new();

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is string pageKey)
            NavigateTo(pageKey);
        else
            NavigateTo("News/Default"); // Default page
    }

    public void NavigateTo(string pageKey, object parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter); // Default page
        if (pageKey == "News/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add("News");
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

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length >= 1 && routes[0] == "News")
            NavigateTo("News/Default");
        else
            NavigateTo(string.Join('/', routes));
    }
}
