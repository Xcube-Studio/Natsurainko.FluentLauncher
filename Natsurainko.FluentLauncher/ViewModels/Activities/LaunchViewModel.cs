using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Components.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class LaunchViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<LaunchProcess> LaunchProcesses { get; init; }

    public LaunchViewModel(LaunchService launchService)
    {
        LaunchProcesses = launchService.LaunchProcesses;
        TipVisibility = LaunchProcesses.Any()
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    [ObservableProperty]
    private Visibility tipVisibility;

    [RelayCommand]
    public void Home() => Views.ShellPage.ContentFrame.Navigate(App.GetService<AppearanceService>().HomePageType);
}
