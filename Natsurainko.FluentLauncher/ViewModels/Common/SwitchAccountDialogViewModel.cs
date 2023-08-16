using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class SwitchAccountDialogViewModel : SettingsViewModelBase, ISettingsViewModel
{
    #region Settings

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RemoveCommand))]
    private Account activeAccount;

    #endregion

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

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
    public void Confirm() => _accountService.Activate(ActiveAccount);

    [RelayCommand(CanExecute = nameof(EnableRemoveAccount))]
    public void Remove()
    {
        _accountService.Remove(ActiveAccount);
        ActiveAccount = _accountService.ActiveAccount;
    }

    private bool EnableRemoveAccount() => Accounts.Count >= 2;
}
