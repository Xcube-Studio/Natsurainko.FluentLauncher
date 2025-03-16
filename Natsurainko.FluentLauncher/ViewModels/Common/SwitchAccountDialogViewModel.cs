using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Authentication;
using System.Collections.ObjectModel;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class SwitchAccountDialogViewModel : DialogVM, ISettingsViewModel
{
    #region Settings

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
    public partial Account ActiveAccount { get; set; }

    #endregion

    public ReadOnlyObservableCollection<Account> Accounts { get; set; }

    private readonly AccountService _accountService;

    public SwitchAccountDialogViewModel(SettingsService settingsService, AccountService accountService)
    {
        _settingsService = settingsService;
        _accountService = accountService;
        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        (this as ISettingsViewModel).InitializeSettings();
    }

    [RelayCommand]
    void Confirm() => _accountService.ActivateAccount(ActiveAccount);

    [RelayCommand(CanExecute = nameof(EnableRemoveAccount))]
    void Remove()
    {
        _accountService.RemoveAccount(ActiveAccount);
        ActiveAccount = _accountService.ActiveAccount;
    }

    private bool EnableRemoveAccount() => Accounts.Count >= 2;

    ~SwitchAccountDialogViewModel()
    {
        if (this is ISettingsViewModel settingsViewModel)
            settingsViewModel.RemoveSettingsChagnedHandlers();
    }
}
