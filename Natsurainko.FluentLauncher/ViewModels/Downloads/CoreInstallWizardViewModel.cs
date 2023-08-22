using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Classes.Data.Download;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.ViewModels.CoreInstallWizard;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Downloads;
using Nrk.FluentCore.Classes.Datas.Download;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads;

internal partial class CoreInstallWizardViewModel : ObservableObject
{
    [ObservableProperty]
    private WizardViewModelBase currentFrameDataContext;

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();

    private readonly NotificationService _notificationService;
    private readonly VersionManifestItem _manifestItem;

    private Frame _contentFrame;

    public CoreInstallWizardViewModel(VersionManifestItem manifestItem)
    {
        _manifestItem = manifestItem;
        _notificationService = App.GetService<NotificationService>();
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _contentFrame = grid.FindName("contentFrame") as Frame;

        CurrentFrameDataContext = new EnterCoreSettingsViewModel(_manifestItem);

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
        if (CurrentFrameDataContext.GetType().Equals(typeof(ConfirmProfileViewModel)))
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
    public void Cancel() => ShellPage.ContentFrame.Navigate(typeof(ResourcesSearchPage), new ResourceSearchData
    {
        SearchInput = _manifestItem.Id,
        ResourceType = 0,
    });

    private void Finish()
    {
        /*
        _notificationService.NotifyWithSpecialContent(
            $"Added Successfully",
            "AuthenticationSuccessfulNotifyTemplate",
            account, "\uE73E");*/
    }

}
