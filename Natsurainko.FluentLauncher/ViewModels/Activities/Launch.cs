using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

partial class LaunchViewModel : ObservableObject
{
    [ObservableProperty]
    private List<LaunchArrangement> launchArrangements = GlobalActivitiesCache.LaunchArrangements;

    [ObservableProperty]
    private Visibility tipVisibility = GlobalActivitiesCache.LaunchArrangements.Any()
        ? Visibility.Collapsed
        : Visibility.Visible;

    [RelayCommand]
    public void Home() => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Home.HomePage));
}
