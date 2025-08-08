using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Launch;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class LaunchViewModel(LaunchService launchService, IDialogActivationService<ContentDialogResult> dialogActivationService) 
    : PageVM, INavigationAware
{
    public ReadOnlyObservableCollection<LaunchTaskViewModel> Tasks { get; } = new(launchService.LaunchTasks);

    public IDialogActivationService<ContentDialogResult> DialogActivationService { get; } = dialogActivationService;

    [RelayCommand]
    void GoToHome() => GlobalNavigate("HomePage");
}
