using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using System.Collections.Generic;
using System.ComponentModel;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class AuthenticationWizardDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private WizardViewModelBase currentFrameDataContext;

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();

    private readonly AccountService _accountService;
    private readonly NotificationService _notificationService;

    private Frame _contentFrame;
    private ContentDialog _dialog;

    public AuthenticationWizardDialogViewModel()
    {
        _accountService = App.GetService<AccountService>();
        _notificationService = App.GetService<NotificationService>();
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _contentFrame = grid.FindName("contentFrame") as Frame;
        _dialog = grid.FindName("Dialog") as ContentDialog;

        CurrentFrameDataContext = new ChooseAccountTypeViewModel();

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
        var vm = CurrentFrameDataContext as ConfirmProfileViewModel;
        var account = vm.SelectedAccount;

        _accountService.AddAccount(account);
        _accountService.Activate(account);

        _dialog.Hide();

        _notificationService.NotifyWithSpecialContent(
            $"Added Successfully",
            "AuthenticationSuccessfulNotifyTemplate",
            account, "\uE73E");
    }
}
