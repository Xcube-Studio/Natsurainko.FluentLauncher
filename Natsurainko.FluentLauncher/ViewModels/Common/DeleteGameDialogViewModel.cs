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
using Nrk.FluentCore.Management;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class DeleteGameDialogViewModel : ObservableObject
{
    private readonly MinecraftInstance _MinecraftInstance;
    private readonly INavigationService _navigationService;

    private ContentDialog _dialog;

    public string Title => $"\"{_MinecraftInstance.GetConfig().NickName}\"";

    [ObservableProperty]
    private bool deleteCoreSettings = true;

    public DeleteGameDialogViewModel(MinecraftInstance MinecraftInstance, INavigationService navigationService)
    {
        _MinecraftInstance = MinecraftInstance;
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

        var directory = new DirectoryInfo(Path.Combine(_MinecraftInstance.MinecraftFolderPath, "versions", _MinecraftInstance.InstanceId));
        var notificationService = App.GetService<NotificationService>();

        if (!ExistsOccupiedFile(directory))
        {
            _MinecraftInstance.Delete();

            if (DeleteCoreSettings)
            {
                var file = _MinecraftInstance.GetConfig().FilePath;
                if (File.Exists(file)) File.Delete(file);
            }

            App.DispatcherQueue.TryEnqueue(() =>
            {
                var gameService = App.GetService<GameService>();
                gameService.RefreshGames();

                _dialog.Hide();
                _navigationService.Parent!.NavigateTo("CoresPage");
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
