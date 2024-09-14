using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class DownloadViewModel : ObservableObject, INavigationAware
{
    public ReadOnlyObservableCollection<TaskViewModel> Tasks { get; }

    public INavigationService NavigationService { get; init; }


    public DownloadViewModel(DownloadService downloadService, INavigationService navigationService)
    {
        NavigationService = navigationService;
        Tasks = new(downloadService.DownloadTasks);
    }

    [RelayCommand]
    void GoToDownload() => NavigationService.NavigateTo("Download/Navigation");
}
