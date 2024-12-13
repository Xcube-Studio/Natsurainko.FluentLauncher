using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class DeleteInstanceDialogViewModel : ObservableObject
{
    private readonly MinecraftInstance _minecraftInstance;
    private readonly INavigationService _navigationService;
    private readonly NotificationService _notificationService;
    private readonly GameService _gameService;

    private ContentDialog _dialog = null!; // Set in LoadEvent

    public string Title => $"\"{_minecraftInstance.InstanceId}\"";

    [ObservableProperty]
    public partial bool DeleteCoreSettings { get; set; } = true;

    public DeleteInstanceDialogViewModel(
        MinecraftInstance minecraftInstance,
        INavigationService navigationService,
        NotificationService notificationService,
        GameService gameService)
    {
        _minecraftInstance = minecraftInstance;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _gameService = gameService;
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _dialog = (ContentDialog)grid.FindName("Dialog");
    }

    [RelayCommand]
    public Task Delete() => Task.Run(() =>
    {
        bool ExistsOccupiedFile(DirectoryInfo directory)
        {
            foreach (var file in directory.EnumerateFiles())
                if (file.IsFileOccupied())
                    return true;

            foreach (var directoryInfo in directory.EnumerateDirectories())
                if (ExistsOccupiedFile(directoryInfo))
                    return true;

            return false;
        }

        var directory = new DirectoryInfo(Path.Combine(_minecraftInstance.MinecraftFolderPath, "versions", _minecraftInstance.InstanceId));

        if (!ExistsOccupiedFile(directory))
        {
            _minecraftInstance.Delete();

            if (DeleteCoreSettings)
            {
                var file = _minecraftInstance.GetConfig().FilePath;
                if (File.Exists(file)) File.Delete(file);
            }

            App.DispatcherQueue.TryEnqueue(() =>
            {
                _gameService.RefreshGames();

                _dialog.Hide();
                _navigationService.Parent!.NavigateTo("CoresPage");
            });

            _notificationService.NotifyWithoutContent(
                ResourceUtils.GetValue("Notifications", "_DeleteGameTitle"),
                icon: "\uE73E");
        }
        else
        {
            App.DispatcherQueue.TryEnqueue(() => _dialog.Hide());

            _notificationService.NotifyMessage(
                ResourceUtils.GetValue("Notifications", "_DeleteGameFailedT"),
                ResourceUtils.GetValue("Notifications", "_DeleteGameFailedD"),
                icon: "\uE711");
        }
    });

    [RelayCommand]
    public void Cancel() => _dialog.Hide();
}
