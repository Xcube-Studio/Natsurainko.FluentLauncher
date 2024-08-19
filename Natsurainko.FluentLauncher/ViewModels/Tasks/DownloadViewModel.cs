using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Network;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class DownloadViewModel : ObservableObject, INavigationAware
{
    private readonly DownloadService _downloadService;
    private readonly INavigationService _navigationService;

    public ObservableCollection<object> Tasks { get; } = [];

    public DownloadViewModel(DownloadService downloadService, INavigationService navigationService)
    {
        _downloadService = downloadService;
        _navigationService = navigationService;

        foreach (var processViewModel in downloadService.DownloadProcesses)
            Tasks.Add(processViewModel);
    }

    [RelayCommand]
    void GoToDownload() => _navigationService.NavigateTo("Download/Navigation");
}
