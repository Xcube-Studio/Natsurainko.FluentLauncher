using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Network;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class DownloadViewModel : ObservableObject, INavigationAware
{
    private readonly DownloadService _downloadService;

    public ObservableCollection<object> Tasks { get; } = [];

    public DownloadViewModel(DownloadService downloadService)
    {
        _downloadService = downloadService;

        foreach (var processViewModel in downloadService.DownloadProcesses)
            Tasks.Add(processViewModel);
    }
}
