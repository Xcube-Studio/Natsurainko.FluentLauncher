using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class DownloadViewModel(DownloadService downloadService) 
    : PageVM, INavigationAware
{
    public ReadOnlyObservableCollection<TaskViewModel> Tasks { get; } = new(downloadService.DownloadTasks);

    [RelayCommand]
    void GoToDownload() => GlobalNavigate("Download/Navigation");
}
