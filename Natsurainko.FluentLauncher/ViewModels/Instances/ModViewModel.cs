using CommunityToolkit.Mvvm.ComponentModel;
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
    public string ModsFolder { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    public bool NotSupportMod => !MinecraftInstance.IsSupportMod();

    public ObservableCollection<ModItemVM> Mods { get; private set; } = [];

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        ModsFolder = MinecraftInstance.GetModsDirectory();

        Directory.CreateDirectory(ModsFolder);

        Task.Run(LoadModsAsync).Forget();
    }

    [RelayCommand]
    async Task OpenModsFolder() => await Launcher.LaunchFolderPathAsync(ModsFolder);

    [RelayCommand]
    void InstallMods() => GlobalNavigate("ModsDownload/Navigation");

    public async Task LoadModsAsync()
    {
        await Dispatcher.EnqueueAsync(Mods.Clear);

        await foreach (var minecraftMod in ModManager.EnumerateModsAsync(ModsFolder))
            await Dispatcher.EnqueueAsync(() => Mods.Add(new ModItemVM(minecraftMod, notificationService)));
    }

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
    void OpenMod(MinecraftMod modInfo) => ExplorerHelper.ShowAndSelectFile(modInfo.AbsolutePath);

    [RelayCommand]
    async Task SearchMcMod(MinecraftMod modInfo) => await Launcher.LaunchUriAsync(new Uri($"https://search.mcmod.cn/s?key={modInfo.DisplayName}"));

    [RelayCommand]
    void DeleteMod(MinecraftMod modInfo)
    {
        File.Delete(modInfo.AbsolutePath);
        Task.Run(LoadModsAsync).Forget();

        notificationService.ModDeleted();
    }
}

internal partial class ModItemVM(MinecraftMod minecraftMod, INotificationService notificationService) : ObservableObject
{
    public MinecraftMod Mod { get; } = minecraftMod;

    [ObservableProperty]
    public partial bool IsOn { get; set; } = minecraftMod.IsEnabled;

    public bool TrySwitchMod(MinecraftMod modInfo, bool isEnable)
    {
        try
        {
            modInfo.Switch(isEnable);
            return true;
        }
        catch (IOException ex) when (ex.HResult == -2147024864)
        {
            notificationService.ModOccupied();
        }
        catch (Exception ex)
        {
            notificationService.ModSwitchFailed(ex);
        }

        return false;
    }

    partial void OnIsOnChanged(bool value)
    {
        if (Mod.IsEnabled != IsOn && !TrySwitchMod(Mod, IsOn))
        {
            IsOn = Mod.IsEnabled; // Reset the toggle state if the switch failed
        }
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

    [Notification<TeachingTip>(Title = "Notifications__ModOccupied", Message = "Notifications__ModOccupiedDescription", Type = NotificationType.Error, Delay = double.NaN)]
    public static partial void ModOccupied(this INotificationService notificationService);

    [ExceptionNotification(Title = "Notifications__ModSwitchFailed")]
    public static partial void ModSwitchFailed(this INotificationService notificationService, Exception exception);
}