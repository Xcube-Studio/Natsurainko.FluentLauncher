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
using Nrk.FluentCore.GameManagement.Mods;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.System;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Instances;

internal partial class ModViewModel(INotificationService notificationService) : PageVM, INavigationAware
{
    public ModManager ModsManager { get; private set; }

    public string ModsFolder { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    public bool NotSupportMod => !MinecraftInstance.IsSupportMod();

    public ObservableCollection<MinecraftMod> Mods { get; private set; } = [];

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        ModsFolder = MinecraftInstance.GetModsDirectory();

        Directory.CreateDirectory(ModsFolder);
        ModsManager = new ModManager(ModsFolder);

        LoadModList();
    }

    [RelayCommand]
    async Task OpenModsFolder() => await Launcher.LaunchFolderPathAsync(ModsFolder);

    [RelayCommand]
    void ConfirmDelete(MinecraftMod modInfo)
    {
        notificationService.Show(new ConfirmNotification
        {
            Title = LocalizedStrings.Notifications__ModDeleteInquire,
            Message = modInfo.AbsolutePath,
            ActionButtonCommand = DeleteModCommand,
            ActionButtonCommandParameter = modInfo,
            ActionButtonStyle = App.Current.Resources["DeleteButtonStyle"] as Style,
            ActionButtonContent = new TextBlock()
            {
                Text = LocalizedStrings.Buttons_Delete_Text,
                Foreground = new SolidColorBrush(Colors.White)
            },
        });
    }

    [RelayCommand]
    void DeleteMod(MinecraftMod modInfo)
    {
        File.Delete(modInfo.AbsolutePath);
        LoadModList();

        notificationService.ModDeleted();
    }

    [RelayCommand]
    void InstallMods() => GlobalNavigate("ModsDownload/Navigation");

    internal async void LoadModList()
    {
        Mods.Clear();

        try
        {
            await foreach (var saveInfo in ModsManager.EnumerateModsAsync())
                await Dispatcher.EnqueueAsync(() => Mods.Add(saveInfo));
        }
        catch { }
    }
}

internal static partial class ModViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__ModDeleted", Type = NotificationType.Success)]
    public static partial void ModDeleted(this INotificationService notificationService);

    [Notification<InfoBar>(Title = "Notifications__ModAdded.Replace(\"${count}\", count.ToString())", Type = NotificationType.Success)]
    public static partial void ModAdded(this INotificationService notificationService, int count);

    [Notification<InfoBar>(Title = "Notifications__ModDrag", Delay = double.NaN)]
    public static partial void ModDrag(this INotificationService notificationService);
}