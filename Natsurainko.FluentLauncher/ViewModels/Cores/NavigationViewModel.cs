﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.XamlHelpers.Converters;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Display.Core;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

public partial class NavigationViewModel : ObservableObject, INavigationAware
{
    public INavigationService NavigationService { get; init; }

    public ObservableCollection<string> DisplayedPath { get; } = new();
    public MinecraftInstance MinecraftInstance { get; private set; } = null!;

    public string InstanceId { get; private set; } = null!; // 缓存游戏名称，防止昵称修改后名称对不上

    public NavigationViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        //if (parameter is null)
        //    throw new ArgumentNullException(nameof(parameter));

        //MinecraftInstance = (MinecraftInstance)parameter;
        //InstanceId = MinecraftInstance.GetDisplayName();

        NavigateTo("Cores/Default");
    }

    //public void HandleBreadcrumbBarLoading(object args)
    //{
    //    var converter = (BreadcrumbBarLocalizationConverter)args;
    //    converter.IgnoredText.Add(InstanceId);
    //}

    public void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length >= 1 && routes[0] == "Cores")
            NavigateTo("Cores/Default");
        //else if (routes[^1] == InstanceId)
        //    NavigateTo("Core/Instance", MinecraftInstance);
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
            //DisplayedPath.Add(InstanceId);
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
