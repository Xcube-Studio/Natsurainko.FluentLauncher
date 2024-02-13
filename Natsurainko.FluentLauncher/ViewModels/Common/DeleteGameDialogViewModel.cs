using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.Launch;
using Nrk.FluentCore.Utils;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class DeleteGameDialogViewModel : ObservableObject
{
    private readonly GameInfo _gameInfo;
    private readonly INavigationService _navigationService;

    private ContentDialog _dialog;

    public string Title => $"\"{_gameInfo.Name}\"";

    [ObservableProperty]
    private bool deleteCoreSettings = true;

    public DeleteGameDialogViewModel(GameInfo gameInfo, INavigationService navigationService)
    {
        _gameInfo = gameInfo;
        _navigationService = navigationService;
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _dialog = grid.FindName("Dialog") as ContentDialog;
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

        var directory = new DirectoryInfo(Path.Combine(_gameInfo.MinecraftFolderPath, "versions", _gameInfo.AbsoluteId));
        var notificationService = App.GetService<NotificationService>();

        if (!ExistsOccupiedFile(directory))
        {
            _gameInfo.Delete();

            if (DeleteCoreSettings)
            {
                var file = _gameInfo.GetSpecialConfig().FilePath;
                if (File.Exists(file)) File.Delete(file);
            }

            App.DispatcherQueue.TryEnqueue(() =>
            {
                var gameService = App.GetService<GameService>();
                gameService.RefreshGames();

                _dialog.Hide();
                _navigationService.Parent.NavigateTo("CoresPage");
            });

            notificationService.NotifyWithoutContent(
                ResourceUtils.GetValue("Notifications", "_DeleteGameTitle"),
                icon: "\uE73E");
        }
        else
        {
            App.DispatcherQueue.TryEnqueue(() => _dialog.Hide());

            notificationService.NotifyMessage(
                ResourceUtils.GetValue("Notifications", "_DeleteGameFailedT"),
                ResourceUtils.GetValue("Notifications", "_DeleteGameFailedD"),
                icon: "\uE711");
        }
    });

    [RelayCommand]
    public void Cancel() => _dialog.Hide();
}
