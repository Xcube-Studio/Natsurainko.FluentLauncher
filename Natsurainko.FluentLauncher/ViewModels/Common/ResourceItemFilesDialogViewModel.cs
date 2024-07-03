using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class ResourceItemFilesDialogViewModel : ObservableObject
{
    private readonly object _resource;
    private ContentDialog _dialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    private object selectedFile;

    private readonly InterfaceCacheService _interfaceCacheService = App.GetService<InterfaceCacheService>();
    private readonly DownloadService _downloadService = App.GetService<DownloadService>();
    private readonly GameService _gameService = App.GetService<GameService>();
    private readonly INavigationService _navigationService;

    public bool CanConfirm => SelectedFile != null;

    public ResourceItemFilesDialogViewModel(object resource, INavigationService navigationService)
    {
        _resource = resource;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public Task LoadEvent(object args) => Task.Run(async () =>
    {
        App.DispatcherQueue.TryEnqueue(() =>
        {
            var grid = args.As<Grid, object>().sender;
            _dialog = grid.FindName("Dialog") as ContentDialog;
        });

        IEnumerable<object> files = default;

        if (_resource is CurseForgeResource curseResource)
        {
            files = from item in curseResource.Files
                    group item by item.McVersion into g
                    orderby g.Key
                    select new GroupInfoList(g) { Key = g.Key };
        }
        else if (_resource is ModrinthResource modrinthResource)
        {
            files = from item in await _interfaceCacheService.ModrinthClient.GetProjectVersionsAsync(modrinthResource.Id)
                    group item by item.McVersion into g
                    orderby g.Key
                    select new GroupInfoList(g) { Key = g.Key };
        }

        App.DispatcherQueue.TryEnqueue(() =>
        {
            var grid = args.As<Grid, object>().sender;

            var collectionViewSource = grid.Resources["CollectionViewSource"] as CollectionViewSource;
            collectionViewSource.Source = files;
        });
    });

    [RelayCommand]
    public void Cancel() => _dialog.Hide();

    [RelayCommand]
    public void Confirm()
    {
        var saveFileDialog = new SaveFileDialog
        {
            FileName = SelectedFile is CurseForgeFile curse ? curse.FileName : ((ModrinthFile)SelectedFile).FileName,
            InitialDirectory = _gameService.ActiveMinecraftFolder
        };

        if (saveFileDialog.ShowDialog().GetValueOrDefault())
        {
            _downloadService.DownloadResourceFile(SelectedFile, saveFileDialog.FileName);
            _dialog.Hide();

            _navigationService.NavigateTo("ActivitiesNavigationPage", "DownloadTasksPage");
        }
    }
}
