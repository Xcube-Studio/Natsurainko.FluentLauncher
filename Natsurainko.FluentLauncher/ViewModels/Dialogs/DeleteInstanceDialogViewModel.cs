using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class DeleteInstanceDialogViewModel(
    NotificationService notificationService,
    GameService gameService) : DialogVM
{
    private MinecraftInstance _minecraftInstance = null!;

    [ObservableProperty]
    public partial bool DeleteCoreSettings { get; set; } = true;

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

            if (DeleteCoreSettings)
            {
                var file = _minecraftInstance.GetConfig().FilePath;
                if (File.Exists(file)) File.Delete(file);
            }

            await Dispatcher.EnqueueAsync(() =>
            {
                gameService.RefreshGames();

                HideAndGlobalNavigate("Cores/Navigation");
            });

            notificationService.NotifyWithoutContent(
                LocalizedStrings.Notifications__DeleteGameTitle,
                icon: "\uE73E");
        }
        else
        {
            Dispatcher.TryEnqueue(() => View.Hide());

            notificationService.NotifyMessage(
                LocalizedStrings.Notifications__DeleteGameFailedT,
                LocalizedStrings.Notifications__DeleteGameFailedD,
                icon: "\uE711");
        }
    });

    [RelayCommand]
    void Cancel() => View.Hide();
}
