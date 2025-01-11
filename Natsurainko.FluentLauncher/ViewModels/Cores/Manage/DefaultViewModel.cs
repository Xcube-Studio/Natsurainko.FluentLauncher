using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.StartScreen;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly NotificationService _notificationService;
    private readonly QuickLaunchService _quickLaunchService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    private JumpList jumpList;

    public MinecraftInstance MinecraftInstance { get; private set; }

    [ObservableProperty]
    public partial InstanceConfig InstanceConfig { get; set; }

    public DefaultViewModel(
        INavigationService navigationService, 
        NotificationService notificationService,
        QuickLaunchService quickLaunchService,
        IDialogActivationService<ContentDialogResult> dialogs)
    {
        _navigationService = navigationService;
        _notificationService = notificationService;
        _quickLaunchService = quickLaunchService;
        _dialogs = dialogs;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormatSize))]
    public partial GameStorageInfo GameStorageInfo { get; set; }

    [ObservableProperty]
    public partial bool Pinned { get; set; }

    public string FormatSize
    {
        get
        {
            if (GameStorageInfo == null)
                return string.Empty;

            double d = GameStorageInfo.TotalSize;
            int i = 0;

            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }

            var unit = new string[] { "B", "KB", "MB", "GB", "TB" };
            return string.Format("{0} {1}", Math.Round(d, 2), unit[i]);
        }
    }

    async void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        InstanceConfig = MinecraftInstance.GetConfig();

        _ = Task.Run(() =>
        {
            var gameStorageInfo = MinecraftInstance.GetStatistics();
            App.DispatcherQueue.TryEnqueue(() => GameStorageInfo = gameStorageInfo);
        });

        jumpList = await JumpList.LoadCurrentAsync();
        Pinned = _quickLaunchService.IsExisted(jumpList, MinecraftInstance, out var item, QuickLaunchService.PinnedUri);

        InstanceConfig.PropertyChanged += InstanceConfig_PropertyChanged;
    }

    private void InstanceConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NickName" && !string.IsNullOrEmpty(InstanceConfig.NickName))
            InstanceConfig.NickName = InstanceConfig.NickName;
    }

    [RelayCommand]
    void CardClick(string tag) => _navigationService.NavigateTo(tag, MinecraftInstance);

    [RelayCommand]
    public async Task OpenVersionFolder() => await Launcher.LaunchFolderPathAsync(MinecraftInstance.GetGameDirectory());

    [RelayCommand]
    public async Task DeleteGame() => await _dialogs.ShowAsync("DeleteInstanceDialog", (MinecraftInstance, _navigationService));

    protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Pinned) && jumpList != null)
        {
            if (Pinned)
                await _quickLaunchService.AddPinMinecraftInstance(MinecraftInstance);
            else if (!Pinned && _quickLaunchService.IsExisted(jumpList, MinecraftInstance, out var jumpListItem, QuickLaunchService.PinnedUri))
            {
                jumpList.Items.Remove(jumpListItem);
                await jumpList.SaveAsync();
            }
        }
    }
}
