using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Tasks;

internal partial class DownloadViewModel : ObservableObject, INavigationAware
{
    private readonly DownloadService _downloadService;

    public ObservableCollection<object> Tasks { get; } = [];

    public DownloadViewModel(DownloadService downloadService)
    {
        _downloadService = downloadService;
    }


}
