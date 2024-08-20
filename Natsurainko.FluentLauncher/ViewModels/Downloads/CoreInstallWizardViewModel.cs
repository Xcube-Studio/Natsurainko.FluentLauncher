using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;
using Nrk.FluentCore.Experimental.GameManagement.Downloader;
using Nrk.FluentCore.Resources;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CoreInstallWizardViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty]
    private WizardViewModelBase currentFrameDataContext;

    [ObservableProperty]
    private string[] breadcrumbBarItemsSource;

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();

    private readonly NotificationService _notificationService;
    private readonly DownloadService _downloadService;
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;

    private readonly CurseForgeClient _curseForgeClient;

    private VersionManifestItem _manifestItem;

    private Frame _contentFrame;

    public CoreInstallWizardViewModel(
        INavigationService navigationService,
        NotificationService notificationService,
        DownloadService downloadService,
        GameService gameService,
        CurseForgeClient curseForgeClient)
    {
        _notificationService = notificationService;
        _downloadService = downloadService;
        _navigationService = navigationService;
        _gameService = gameService;

        _curseForgeClient = curseForgeClient;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        _manifestItem = (parameter as VersionManifestItem) ?? throw new InvalidDataException();
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _contentFrame = grid.FindName("contentFrame") as Frame;

        BreadcrumbBarItemsSource = new string[]
        {
            ResourceUtils.GetValue("Downloads", "CoreInstallWizardPage", "_BreadcrumbBar_First"),
            _manifestItem.Id
        };

        CurrentFrameDataContext = new ChooseModLoaderViewModel(_manifestItem);

        _contentFrame.Navigate(
            CurrentFrameDataContext.XamlPageType,
            null,
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    /// <summary>
    /// Next Button Command
    /// </summary>
    [RelayCommand]
    public void Next()
    {
        if (CurrentFrameDataContext.GetType().Equals(typeof(AdditionalOptionsViewModel)))
        {
            Finish();
            return;
        }

        _contentFrame.Content = null;

        _viewModelStack.Push(CurrentFrameDataContext);
        CurrentFrameDataContext = CurrentFrameDataContext.GetNextViewModel();

        _contentFrame.Navigate(
            CurrentFrameDataContext.XamlPageType,
            null,
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    /// <summary>
    /// Back Button Command
    /// </summary>
    [RelayCommand]
    public void Previous()
    {
        _contentFrame.Content = null;

        CurrentFrameDataContext = _viewModelStack.Pop();

        _contentFrame.Navigate(
            CurrentFrameDataContext.XamlPageType,
            null,
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
    }

    /// <summary>
    /// Cancel Button Command
    /// </summary>
    [RelayCommand]
    public void Cancel() => _navigationService.GoBack();

    [RelayCommand]
    void BreadcrumbBarClicked(object args)
    {
        var e = args.As<BreadcrumbBar, BreadcrumbBarItemClickedEventArgs>();

        if (e.args.Index.Equals(0))
            _navigationService.GoBack();
    }

    private void Finish()
    {
        var vm = (AdditionalOptionsViewModel)this.CurrentFrameDataContext;
        var installInfo = vm._coreInstallationInfo;
        var stepInstallMod = ResourceUtils.GetValue("Converters", "_ProgressItem_InstallMod");

        if (vm.EnabledFabricApi)
        {
            var file = installInfo.EnableIndependencyCore
                ? Path.Combine(_gameService.ActiveMinecraftFolder, "versions", installInfo.AbsoluteId, "mods", vm.FabricApi.FileName)
                : Path.Combine(_gameService.ActiveMinecraftFolder, "mods", vm.FabricApi.FileName);

            installInfo.AdditionalOptions.Add(new(@this =>
            {
                var downloadTask = _downloadService.Downloader.CreateDownloadTask(
                    vm.FabricApi.Url,
                    file);

                Timer t = new((_) =>
                {
                    if (downloadTask.TotalBytes is null)
                        return;
                    @this.OnProgressChanged(downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                downloadTask.StartAsync().Wait();
                t.Dispose();

            }, stepInstallMod.Replace("${file}", vm.FabricApi.FileName)));
        }

        if (vm.EnabledOptiFabric)
        {
            var file = installInfo.EnableIndependencyCore
                ? Path.Combine(_gameService.ActiveMinecraftFolder, "versions", installInfo.AbsoluteId, "mods", vm.OptiFabric.FileName)
                : Path.Combine(_gameService.ActiveMinecraftFolder, "mods", vm.OptiFabric.FileName);

            installInfo.AdditionalOptions.Add(new(async @this =>
            {
                string fileUrl = await _curseForgeClient.GetFileUrlAsync(vm.OptiFabric);

                var downloadTask = _downloadService.Downloader.CreateDownloadTask(
                    fileUrl,
                    file);

                using Timer t = new((_) =>
                {
                    if (downloadTask.TotalBytes is null)
                        return;
                    @this.OnProgressChanged(downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                await downloadTask.StartAsync();

            }, stepInstallMod.Replace("${file}", vm.OptiFabric.FileName)));
        }

        _downloadService.InstallCore(installInfo);
        _navigationService.NavigateTo("Tasks/Download");
    }
}
