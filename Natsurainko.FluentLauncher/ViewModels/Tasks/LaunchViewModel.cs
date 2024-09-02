using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class LaunchViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public ReadOnlyObservableCollection<LaunchTaskViewModel> Tasks { get; }

    public LaunchViewModel(LaunchService launchService, INavigationService navigationService)
    {
        _navigationService = navigationService;
        Tasks = new(launchService.LaunchTasks);
    }

    [RelayCommand]
    void GoToHome() => _navigationService.NavigateTo("HomePage");
}
