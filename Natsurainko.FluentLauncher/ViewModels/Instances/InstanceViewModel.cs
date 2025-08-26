using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Instances;

internal partial class InstanceViewModel(
    INavigationService navigationService,
    QuickLaunchService quickLaunchService,
    IDialogActivationService<ContentDialogResult> dialogs) : PageVM, INavigationAware
{
    private readonly IDialogActivationService<ContentDialogResult> _dialogs = dialogs;

    private JumpList jumpList;

    public MinecraftInstance MinecraftInstance { get; private set; }

    [ObservableProperty]
    public partial InstanceConfig InstanceConfig { get; set; }

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

            return LongExtensions.ToFileSizeString(GameStorageInfo.TotalSize);
        }
    }

    async partial void OnPinnedChanged(bool value)
    {
        if (jumpList == null) return;

        if (value)
            await quickLaunchService.AddPinMinecraftInstance(MinecraftInstance);
        else if (quickLaunchService.IsExisted(jumpList, MinecraftInstance, out var jumpListItem, QuickLaunchService.PinnedUri))
        {
            jumpList.Items.Remove(jumpListItem);
            await jumpList.SaveAsync();
        }
    }

    async void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        InstanceConfig = MinecraftInstance.GetConfig();

        Task.Run(() =>
        {
            var gameStorageInfo = MinecraftInstance.GetStatistics();
            Dispatcher.TryEnqueue(() => GameStorageInfo = gameStorageInfo);
        }).Forget();

        jumpList = await JumpList.LoadCurrentAsync();
        Pinned = quickLaunchService.IsExisted(jumpList, MinecraftInstance, out var item, QuickLaunchService.PinnedUri);

        InstanceConfig.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "NickName" && !string.IsNullOrEmpty(InstanceConfig.NickName))
                InstanceConfig.NickName = InstanceConfig.NickName;
        };
    }

    [RelayCommand]
    void CardClick(string tag) => navigationService.NavigateTo(tag, MinecraftInstance);

    [RelayCommand]
    void OpenVersionFolder() => ExplorerHelper.OpenFolder(MinecraftInstance.GetGameDirectory());

    [RelayCommand]
    async Task DeleteGame() => await _dialogs.ShowAsync("DeleteInstanceDialog", MinecraftInstance);
}
