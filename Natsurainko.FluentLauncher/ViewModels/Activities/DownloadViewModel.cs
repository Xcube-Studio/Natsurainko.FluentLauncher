using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Download;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class DownloadViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<DownloadProcess> DownloadProcesses { get; init; }

    [ObservableProperty]
    private Visibility tipVisibility;

    public DownloadViewModel(DownloadService downloadService)
    {
        DownloadProcesses = downloadService.DownloadProcesses;
        TipVisibility = DownloadProcesses.Any()
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    [RelayCommand]
    public void Resource() => Views.ShellPage.ContentFrame.Navigate(typeof(Views.Downloads.DownloadsPage));
}
