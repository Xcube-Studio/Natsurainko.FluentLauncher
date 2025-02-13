using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Nrk.FluentCore.GameManagement.Installer;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;

internal partial class NavigationViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService { get; init; }

    public ObservableCollection<string> DisplayedPath { get; } = [];

    public VersionManifestItem? CurrentInstance { get; set; }

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("InstancesDownload/Default", parameter); // Default page

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length == 1 && routes[0] == "InstancesDownload")
            NavigateTo("InstancesDownload/Default");
        else
            NavigateTo(string.Join('/', routes));
    }

    private void NavigateTo(string pageKey, object? parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter); // Default page
        if (pageKey == "InstancesDownload/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add("InstancesDownload");
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
