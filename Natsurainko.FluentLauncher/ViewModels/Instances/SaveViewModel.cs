using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.GameManagement.Saves;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Instances;

internal partial class SaveViewModel(INotificationService notificationService) : PageVM, INavigationAware
{
    public MinecraftInstance MinecraftInstance { get; private set; }

    public string SavesFolder { get; private set; }

    public ObservableCollection<SaveInfo> Saves { get; private set; } = [];

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        SavesFolder = MinecraftInstance.GetSavesDirectory();

        Directory.CreateDirectory(SavesFolder);

        Task.Run(ReloadSavesAsync).Forget();
    }

    public async Task ReloadSavesAsync()
    {
        await Dispatcher.EnqueueAsync(Saves.Clear);

        if (!Directory.Exists(SavesFolder))
            return;

        var manager = new SaveManager(SavesFolder);

        await foreach (var saveInfo in manager.EnumerateSavesAsync())
            await Dispatcher.EnqueueAsync(() => Saves.Add(saveInfo));
    }

    [RelayCommand]
    void OpenSavesFolder() => ExplorerHelper.OpenFolder(SavesFolder);

    [RelayCommand]
    void OpenSaveFolder(SaveInfo saveInfo) => ExplorerHelper.OpenFolder(saveInfo.Folder);

    [RelayCommand]
    void ConfirmDelete(SaveInfo saveInfo)
    {
        notificationService.Show(new ConfirmNotification
        {
            Title = "Delete World?",
            Message = $"{saveInfo.LevelName} ({saveInfo.FolderName})",
            ActionButtonCommand = DeleteSaveCommand,
            ActionButtonCommandParameter = saveInfo,
            ActionButtonStyle = App.Current.Resources["DeleteButtonStyle"] as Style,
            ActionButtonContent = new TextBlock
            {
                Text = LocalizedStrings.Buttons_Delete_Text,
                Foreground = new SolidColorBrush(Colors.White)
            }
        });
    }

    [RelayCommand]
    void DeleteSave(SaveInfo saveInfo)
    {
        try
        {
            if (Directory.Exists(saveInfo.Folder))
                Directory.Delete(saveInfo.Folder, true);

            Task.Run(ReloadSavesAsync).Forget();
            notificationService.Show<InfoBar>(new DefaultNotification
            {
                Title = "Deleted",
                Message = $"Deleted world {saveInfo.LevelName}",
                Type = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            notificationService.Show<InfoBar>(new ExceptionNotification
            {
                Title = "Delete Failed",
                Message = ex.Message,
                Exception = ex
            });
        }
    }

    [RelayCommand]
    async Task BackupSave(SaveInfo saveInfo)
    {
        try
        {
            string backupsDir = Path.Combine(SavesFolder, "backups");
            Directory.CreateDirectory(backupsDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupZip = Path.Combine(backupsDir, $"{saveInfo.FolderName}_{timestamp}.zip");

            notificationService.Show<InfoBar>(new DefaultNotification
            {
                Title = "Backing up",
                Message = $"Creating backup for {saveInfo.LevelName}...",
                Type = NotificationType.Info
            });

            await Task.Run(() => ZipFile.CreateFromDirectory(saveInfo.Folder, backupZip));

            notificationService.Show<InfoBar>(new DefaultNotification
            {
                Title = "Backup Created",
                Message = $"Saved to backups/{Path.GetFileName(backupZip)}",
                Type = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            notificationService.Show<InfoBar>(new ExceptionNotification
            {
                Title = "Backup Failed",
                Message = ex.Message,
                Exception = ex
            });
        }
    }
}
