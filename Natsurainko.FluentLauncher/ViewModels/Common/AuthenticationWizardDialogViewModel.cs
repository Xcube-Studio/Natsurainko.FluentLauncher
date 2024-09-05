﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using Nrk.FluentCore.Authentication;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class AuthenticationWizardDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private WizardViewModelBase? currentFrameDataContext;

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();

    private readonly AccountService _accountService;
    private readonly NotificationService _notificationService;
    private readonly AuthenticationService _authService;

    private Frame _contentFrame = null!; // Set in LoadEvent
    private ContentDialog _dialog = null!; // Set in LoadEvent

    public AuthenticationWizardDialogViewModel(AccountService accountService, NotificationService notificationService, AuthenticationService authService)
    {
        _accountService = accountService;
        _notificationService = notificationService;
        _authService = authService;
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _contentFrame = (Frame)grid.FindName("contentFrame");
        _dialog = (ContentDialog)grid.FindName("Dialog");

        CurrentFrameDataContext = new ChooseAccountTypeViewModel(_authService);

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
    public void Cancel() => _dialog.Hide();

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
    }

    private void Finish()
    {
        var vm = (ConfirmProfileViewModel)CurrentFrameDataContext;
        var account = vm.SelectedAccount;

        var existedAccounts = _accountService.Accounts.Where(x => x.Equals(account)).ToArray();

        if (existedAccounts.Length != 0)
        {
            foreach (var item in existedAccounts)
            {
                _accountService.RemoveAccount(item, true);
            }

            _notificationService.NotifyWithoutContent(
                ResourceUtils.GetValue("Notifications", "_AccountExisted"),
                icon: "\uecc5");
        }

        _accountService.AddAccount(account);
        _accountService.ActivateAccount(account);

        _dialog.Hide();

        _notificationService.NotifyWithSpecialContent(
            ResourceUtils.GetValue("Notifications", "_AccountAddSuccessful"),
            "AuthenticationSuccessfulNotifyTemplate",
            account, "\uE73E");
    }
}
