using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Download;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

internal partial class DownloadViewModel : ObservableObject
{
    public ReadOnlyObservableCollection<DownloadProcess> DownloadProcesses { get; init; }

    [ObservableProperty]
    private Visibility tipVisibility;

    private readonly INavigationService _shellNavigationService;

    public DownloadViewModel(DownloadService downloadService, INavigationService navigationService)
    {
        DownloadProcesses = downloadService.DownloadProcesses;
        TipVisibility = DownloadProcesses.Any()
            ? Visibility.Collapsed
            : Visibility.Visible;

        _shellNavigationService = navigationService.Parent ?? throw new InvalidOperationException("Cannot obtain the shell navigaiton service");
    }

    [RelayCommand]
    public void Resource() => _shellNavigationService.NavigateTo("ResourcesDownloadPage");
}
