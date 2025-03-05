using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Nrk.FluentCore.GameManagement.Instances;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService { get; init; }

    public ObservableCollection<string> DisplayedPath { get; } = new();

    public MinecraftInstance? CurrentInstance { get; set; }

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is MinecraftInstance instance)
        {
            CurrentInstance = instance;
            NavigateTo("Cores/Instance", instance);
        }
        else if (parameter is string pageKey)
        {
            NavigateTo(pageKey);
        }
        else
        {
            NavigateTo("Cores/Default");
        }
    }

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length == 1 && routes[0] == "Cores")
            NavigateTo("Cores/Default");
        else if (routes.Length == 2)
            NavigateTo("Cores/Instance", CurrentInstance);
        else
            NavigateTo(string.Join('/', routes));
    }

    public void NavigateTo(string pageKey, object? parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter); // Default page
        if (pageKey == "Cores/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add("Cores");
        }
        else if (pageKey == "Cores/Instance")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add("Cores");
            DisplayedPath.Add(((MinecraftInstance)parameter!).InstanceId);
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
