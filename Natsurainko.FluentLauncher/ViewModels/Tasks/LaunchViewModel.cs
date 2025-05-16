using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Launch;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class LaunchViewModel(LaunchService launchService) 
    : PageVM, INavigationAware
{
    public ReadOnlyObservableCollection<LaunchTaskViewModel> Tasks { get; } = new(launchService.LaunchTasks);

    [RelayCommand]
    void GoToHome() => GlobalNavigate("HomePage");
}
