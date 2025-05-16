using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using Natsurainko.FluentLauncher.Views.Dialogs;
using Nrk.FluentCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class AuthenticateDialogViewModel(
    AccountService accountService, 
    INotificationService notificationService, 
    AuthenticationService authService) : DialogVM<AuthenticateDialog>
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
            notificationService.ProfileConfirmationFailed(ex);

            return;
        }

        var existedAccounts = accountService.Accounts.Where(x => x.ProfileEquals(account)).ToArray();

        if (existedAccounts.Length != 0)
        {
            foreach (var item in existedAccounts)
                accountService.RemoveAccount(item, true);

            notificationService.AccountExisited();
        }

        accountService.AddAccount(account);
        accountService.ActivateAccount(account);

        this.Dialog.Hide();

        notificationService.AccountAdded(account.Name);
    }
}

internal static partial class AuthenticateDialogViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__AccountExisted")]
    public static partial void AccountExisited(this INotificationService notificationService);

    [Notification<InfoBar>(Title = "Notifications__AccountAdded", Message = "Notifications__AccountAddedDescription.Replace(\"${name}\", name)", Type = NotificationType.Success)]
    public static partial void AccountAdded(this INotificationService notificationService, string name);

    [ExceptionNotification(Title = "Notifications__AccountYggdrasilProfileConfirmationFailed")]
    public static partial void ProfileConfirmationFailed(this INotificationService notificationService, Exception exception);
}