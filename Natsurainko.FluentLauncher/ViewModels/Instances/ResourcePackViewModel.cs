using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Instances;

internal partial class ResourcePackViewModel(
    INotificationService notificationService,
    ResourceUpdateService resourceUpdateService,
    DownloadService downloadService) : PageVM, INavigationAware
{
    public string ResourcePacksFolder { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    public ObservableCollection<ResourcePackItem> ResourcePacks { get; private set; } = [];

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        ResourcePacksFolder = Path.Combine(MinecraftInstance.GetGameDirectory(), "resourcepacks");

        Directory.CreateDirectory(ResourcePacksFolder);

        Task.Run(LoadResourcePacksAsync).Forget();
    }

    [RelayCommand]
    void OpenResourcePacksFolder() => ExplorerHelper.OpenFolder(ResourcePacksFolder);

    [RelayCommand]
    void InstallResourcePacks() => GlobalNavigate("TexturePacksDownload/Navigation");

    public async Task LoadResourcePacksAsync()
    {
        await Dispatcher.EnqueueAsync(ResourcePacks.Clear);

        if (!Directory.Exists(ResourcePacksFolder))
            return;

        foreach (var file in Directory.EnumerateFileSystemEntries(ResourcePacksFolder))
        {
            if (File.Exists(file) && (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)))
            {
                var item = new ResourcePackItem(file);
                await Dispatcher.EnqueueAsync(() => ResourcePacks.Add(item));
            }
            else if (Directory.Exists(file))
            {
                var item = new ResourcePackItem(file);
                await Dispatcher.EnqueueAsync(() => ResourcePacks.Add(item));
            }
        }
    }

    [RelayCommand]
    void OpenResourcePack(ResourcePackItem item) => ExplorerHelper.ShowAndSelectFile(item.AbsolutePath);

    [RelayCommand]
    void ConfirmDelete(ResourcePackItem item)
    {
        notificationService.Show(new ConfirmNotification
        {
            Title = "Delete Resource Pack?",
            Message = item.AbsolutePath,
            ActionButtonCommand = DeleteResourcePackCommand,
            ActionButtonCommandParameter = item,
            ActionButtonStyle = App.Current.Resources["DeleteButtonStyle"] as Style,
            ActionButtonContent = new TextBlock
            {
                Text = LocalizedStrings.Buttons_Delete_Text,
                Foreground = new SolidColorBrush(Colors.White)
            }
        });
    }

    [RelayCommand]
    void DeleteResourcePack(ResourcePackItem item)
    {
        try
        {
            item.Delete();
            Task.Run(LoadResourcePacksAsync).Forget();
            notificationService.Show<InfoBar>(new DefaultNotification
            {
                Title = "Success",
                Message = $"Deleted {item.DisplayName}",
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
    async Task CheckUpdate(ResourcePackItem item)
    {
        notificationService.Show<InfoBar>(new DefaultNotification
        {
            Title = "Checking Updates",
            Message = $"Searching update for {item.DisplayName}...",
            Type = NotificationType.Info
        });

        var updateInfo = await resourceUpdateService.CheckForUpdateAsync(
            item.DisplayName,
            null,
            MinecraftInstance != null ? MinecraftInstance.Version.VersionId : "",
            "",
            "resourcepack");

        if (updateInfo != null && updateInfo.HasUpdate && !string.IsNullOrEmpty(updateInfo.DownloadUrl))
        {
            notificationService.Show<InfoBar>(new ActionNotification
            {
                Type = NotificationType.Success,
                Title = "Update Available",
                Message = $"Found newer version: {updateInfo.LatestVersion} ({updateInfo.FileName})",
                GetActionButton = () => new Button
                {
                    Content = "Update Now",
                    Command = new RelayCommand(() =>
                    {
                        if (updateInfo.ModrinthFile != null)
                        {
                            downloadService.DownloadResourceFileAsync(updateInfo.ModrinthFile, ResourcePacksFolder, _ =>
                            {
                                try { item.Delete(); } catch { }
                                Task.Run(LoadResourcePacksAsync).Forget();
                                notificationService.Show<InfoBar>(new DefaultNotification
                                {
                                    Title = "Updated",
                                    Message = $"Updated {item.DisplayName} to {updateInfo.LatestVersion}!",
                                    Type = NotificationType.Success
                                });
                            }).Forget();
                        }
                    })
                }
            });
        }
        else
        {
            notificationService.Show<InfoBar>(new DefaultNotification
            {
                Title = "Up to Date",
                Message = $"{item.DisplayName} is already up to date.",
                Type = NotificationType.Info
            });
        }
    }

    public async Task AddResourcePacksAsync(string[] files)
    {
        int added = 0;
        foreach (var file in files)
        {
            if (File.Exists(file) && (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)))
            {
                string target = Path.Combine(ResourcePacksFolder, Path.GetFileName(file));
                File.Copy(file, target, true);
                added++;
            }
        }

        if (added > 0)
        {
            await LoadResourcePacksAsync();
            notificationService.Show<InfoBar>(new DefaultNotification
            {
                Title = "Added",
                Message = $"Added {added} resource packs.",
                Type = NotificationType.Success
            });
        }
    }
}
