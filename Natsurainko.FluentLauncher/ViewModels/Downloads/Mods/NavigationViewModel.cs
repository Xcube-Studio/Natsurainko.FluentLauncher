﻿using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Data;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

internal class NavigationViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService { get; init; }

    public ObservableCollection<string> DisplayedPath { get; } = new();

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is SearchOptions)
        {
            NavigateTo("ModsDownload/Default", parameter);
        }
        else if (parameter is string pageKey)
        {
            NavigateTo(pageKey);
        }
        else
        {
            NavigateTo("ModsDownload/Default"); // Default page
        }
    }

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length == 1 && routes[0] == "ModsDownload")
            NavigateTo("ModsDownload/Default");
        //else if (routes.Length == 2)
        //    NavigateTo("ModsDownload/Install", CurrentInstance);
        else
            NavigateTo(string.Join('/', routes));
    }

    private void NavigateTo(string pageKey, object? parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter); // Default page
        if (pageKey == "ModsDownload/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add("ModsDownload");
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
