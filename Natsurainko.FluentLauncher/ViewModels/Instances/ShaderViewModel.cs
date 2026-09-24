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

internal partial class ShaderViewModel(
    INotificationService notificationService,
    ResourceUpdateService resourceUpdateService,
    DownloadService downloadService) : PageVM, INavigationAware
{
    public string ShadersFolder { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    public ObservableCollection<ShaderpackItem> Shaders { get; private set; } = [];

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        ShadersFolder = Path.Combine(MinecraftInstance.GetGameDirectory(), "shaderpacks");

        Directory.CreateDirectory(ShadersFolder);

        Task.Run(LoadShadersAsync).Forget();
    }

    [RelayCommand]
    void OpenShadersFolder() => ExplorerHelper.OpenFolder(ShadersFolder);

    [RelayCommand]
    void InstallShaders() => GlobalNavigate("ShadersDownload/Navigation");

    public async Task LoadShadersAsync()
    {
        await Dispatcher.EnqueueAsync(Shaders.Clear);

        if (!Directory.Exists(ShadersFolder))
            return;

        foreach (var file in Directory.EnumerateFileSystemEntries(ShadersFolder))
        {
            if (File.Exists(file) && (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)))
            {
                var item = new ShaderpackItem(file);
                await Dispatcher.EnqueueAsync(() => Shaders.Add(item));
            }
            else if (Directory.Exists(file))
            {
                var item = new ShaderpackItem(file);
                await Dispatcher.EnqueueAsync(() => Shaders.Add(item));
            }
        }
    }

    [RelayCommand]
    void OpenShader(ShaderpackItem item) => ExplorerHelper.ShowAndSelectFile(item.AbsolutePath);

    [RelayCommand]
    void ConfirmDelete(ShaderpackItem item)
    {
        notificationService.Show(new ConfirmNotification
        {
            Title = "Delete Shaderpack?",
            Message = item.AbsolutePath,
            ActionButtonCommand = DeleteShaderCommand,
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
    void DeleteShader(ShaderpackItem item)
    {
        try
        {
            item.Delete();
            Task.Run(LoadShadersAsync).Forget();
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
    async Task CheckUpdate(ShaderpackItem item)
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
            "shader");

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
                            downloadService.DownloadResourceFileAsync(updateInfo.ModrinthFile, ShadersFolder, _ =>
                            {
                                try { item.Delete(); } catch { }
                                Task.Run(LoadShadersAsync).Forget();
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

    public async Task AddShadersAsync(string[] files)
    {
        int added = 0;
        foreach (var file in files)
        {
            if (File.Exists(file) && (file.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase)))
            {
                string target = Path.Combine(ShadersFolder, Path.GetFileName(file));
                File.Copy(file, target, true);
                added++;
            }
        }

        if (added > 0)
        {
            await LoadShadersAsync();
            notificationService.Show<InfoBar>(new DefaultNotification
            {
                Title = "Added",
                Message = $"Added {added} shaderpacks.",
                Type = NotificationType.Success
            });
        }
    }
}
