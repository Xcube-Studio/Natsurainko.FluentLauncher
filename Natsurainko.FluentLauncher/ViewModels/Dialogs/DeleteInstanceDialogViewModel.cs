using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class DeleteInstanceDialogViewModel(
    INotificationService notificationService,
    GameService gameService) : DialogVM
{
    private MinecraftInstance _minecraftInstance = null!;

    [ObservableProperty]
    public partial bool DeleteInstanceSettings { get; set; } = true;

    public string Title => $"\"{_minecraftInstance.InstanceId}\"";

    public override void HandleParameter(object param) => _minecraftInstance = (MinecraftInstance)param;

    [RelayCommand]
    Task Delete() => Task.Run(async () =>
    {
        static bool ExistsOccupiedFile(DirectoryInfo directory)
        {
            foreach (var file in directory.EnumerateFiles())
                if (file.IsFileOccupied())
                    return true;

            foreach (var directoryInfo in directory.EnumerateDirectories())
                if (ExistsOccupiedFile(directoryInfo))
                    return true;

            return false;
        }

        DirectoryInfo directory = new (Path.Combine(_minecraftInstance.MinecraftFolderPath, "versions", _minecraftInstance.InstanceId));

        if (!ExistsOccupiedFile(directory))
        {
            _minecraftInstance.Delete();

            if (DeleteInstanceSettings)
            {
                var file = _minecraftInstance.GetConfig().FilePath;
                if (File.Exists(file)) File.Delete(file);
            }

            await Dispatcher.EnqueueAsync(() =>
            {
                gameService.RefreshGames();
                HideAndGlobalNavigate("Instances/Navigation");
            });

            notificationService.InstanceDeleted();
        }
        else
        {
            await Dispatcher.EnqueueAsync(() => this.Dialog.Hide());
            notificationService.InstanceDeleteFailed();
        }
    });

    [RelayCommand]
    void Cancel() => this.Dialog.Hide();
}

internal static partial class DeleteInstanceDialogViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__InstanceDeleted", Type = NotificationType.Success)]
    public static partial void InstanceDeleted(this INotificationService notificationService);

    [Notification<InfoBar>(Title = "Notifications__InstanceDeleteFailed", Message = "Notifications__InstanceDeleteFailedDescription", Type = NotificationType.Warning)]
    public static partial void InstanceDeleteFailed(this INotificationService notificationService);
}