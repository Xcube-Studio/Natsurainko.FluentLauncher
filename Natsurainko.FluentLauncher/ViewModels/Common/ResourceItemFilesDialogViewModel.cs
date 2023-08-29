using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Services.Download;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Classes.Datas.Download;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class ResourceItemFilesDialogViewModel : ObservableObject
{
    private readonly object _resource;
    private ContentDialog _dialog;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanComfirm))]
    private object selectedFile;

    private readonly InterfaceCacheService _interfaceCacheService = App.GetService<InterfaceCacheService>();
    private readonly DownloadService _downloadService = App.GetService<DownloadService>();
    private readonly GameService _gameService = App.GetService<GameService>();

    public bool CanComfirm => SelectedFile != null;

    public ResourceItemFilesDialogViewModel(object resource)
    {
        _resource = resource;
    }

    [RelayCommand]
    public Task LoadEvent(object args) => Task.Run(() =>
    {
        App.DispatcherQueue.TryEnqueue(() => 
        {
            var grid = args.As<Grid, object>().sender;
            _dialog = grid.FindName("Dialog") as ContentDialog;
        });

        IEnumerable<object> files = default;

        if (_resource is CurseResource curseResource)
        {
            files = from item in curseResource.Files
                    group item by item.McVersion into g
                    orderby g.Key
                    select new GroupInfoList(g) { Key = g.Key };
        }
        else if (_resource is ModrinthResource modrinthResource)
        {
            files = from item in _interfaceCacheService.ModrinthClient.GetProjectVersions(modrinthResource.Id)
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
            FileName = SelectedFile is CurseFile curse ? curse.FileName : ((ModrinthFile)SelectedFile).FileName,
            InitialDirectory = _gameService.ActiveMinecraftFolder
        };

        if (saveFileDialog.ShowDialog().GetValueOrDefault())
        {
            _downloadService.CreateDownloadProcessFromResourceFile(SelectedFile, saveFileDialog.FileName);
            _dialog.Hide();
        }
    }
}
