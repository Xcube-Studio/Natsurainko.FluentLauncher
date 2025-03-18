using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using Natsurainko.FluentLauncher.Views.Dialogs;
using Nrk.FluentCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class AuthenticationWizardDialogViewModel(
    AccountService accountService, 
    NotificationService notificationService, 
    AuthenticationService authService) : DialogVM<AuthenticationWizardDialog>
{
    [ObservableProperty]
    public partial WizardViewModelBase CurrentFrameDataContext { get; set; } = null!;

    private readonly Stack<WizardViewModelBase> _viewModelStack = new();
    private Frame _contentFrame = null!; // Set in LoadEvent

    protected override void OnLoaded()
    {
        _contentFrame = this.Dialog.ContentFrame;

        CurrentFrameDataContext = new ChooseAccountTypeViewModel(authService);

        _contentFrame.Navigate(
            CurrentFrameDataContext.XamlPageType,
            null,
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
    }

    /// <summary>
    /// Next Button Command
    /// </summary>
    [RelayCommand]
    async Task Next()
    {
        if (CurrentFrameDataContext.GetType().Equals(typeof(ConfirmProfileViewModel)))
        {
            await Finish();
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
    void Previous()
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
    void Cancel() => this.Dialog.Hide();

    static string GetAccountTypeName(AccountType accountType)
    {
        string account = LocalizedStrings.Converters__Account;

        if (!ApplicationLanguages.PrimaryLanguageOverride.StartsWith("zh-"))
            account = " " + account;

        return accountType switch
        {
            AccountType.Microsoft => LocalizedStrings.Converters__Microsoft + account,
            AccountType.Yggdrasil => LocalizedStrings.Converters__Yggdrasil + account,
            _ => LocalizedStrings.Converters__Offline + account,
        };
    }

    static string TryGetYggdrasilServerName(Account account)
    {
        if (account is YggdrasilAccount yggdrasilAccount)
        {
            if (yggdrasilAccount.MetaData.TryGetValue("server_name", out var serverName))
                return serverName;
        }

        return string.Empty;
    }

    private async Task Finish()
    {
        var vm = (ConfirmProfileViewModel)CurrentFrameDataContext;
        var account = vm.SelectedAccount!; // checked by ConfirmProfileViewModel.CanNext

        try // refresh accessToken of the selected profile
        {
            if (account is YggdrasilAccount yggdrasilAccount)
                account = await authService.RefreshAsync(yggdrasilAccount);
        } 
        catch (Exception ex)
        {
            this.Dialog.Hide();
            notificationService.NotifyException(LocalizedStrings.Notifications__AccountYggdrasilProfileConfirmationFailed, ex);

            return;
        }

        var existedAccounts = accountService.Accounts.Where(x => x.ProfileEquals(account)).ToArray();

        if (existedAccounts.Length != 0)
        {
            foreach (var item in existedAccounts)
                accountService.RemoveAccount(item, true);

            notificationService.NotifyWithoutContent(
                LocalizedStrings.Notifications__AccountExisted,
                icon: "\uecc5");
        }

        accountService.AddAccount(account);
        accountService.ActivateAccount(account);

        this.Dialog.Hide();

        notificationService.NotifyWithSpecialContent(
            LocalizedStrings.Notifications__AccountAddSuccessful,
            "AuthenticationSuccessfulNotifyTemplate",
            new
            {
                Account = account,
                AccountTypeName = GetAccountTypeName(account.Type),
                YggdrasilServerName = TryGetYggdrasilServerName(account)
            }, "\uE73E");
    }
}
