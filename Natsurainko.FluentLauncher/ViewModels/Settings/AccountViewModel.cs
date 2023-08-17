using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class AccountViewModel : SettingsViewModelBase, ISettingsViewModel
{
    #region Settings

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.EnableDemoUser))]
    private bool enableDemoUser;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.AutoRefresh))]
    private bool autoRefresh;

    #endregion

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    [ObservableProperty]
    private Account activeAccount;

    private readonly AccountService _accountService;
    private readonly AuthenticationService _authenticationService;
    private readonly NotificationService _notificationService;

    public AccountViewModel(
        SettingsService settingsService,
        AccountService accountService,
        AuthenticationService authenticationService,
        NotificationService notificationService)
    {
        _settingsService = settingsService;
        _accountService = accountService;
        _authenticationService = authenticationService;
        _notificationService = notificationService;

        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        WeakReferenceMessenger.Default.Register<ActiveAccountChangedMessage>(this, (r, m) =>
        {
            AccountViewModel vm = r as AccountViewModel;
            vm.ActiveAccount = m.Value;
        });
        (this as ISettingsViewModel).InitializeSettings();
    }

    partial void OnActiveAccountChanged(Account value)
    {
        if (value is not null) _accountService.Activate(value);
    }

    [RelayCommand]
    public void Login() => _ = new AuthenticationWizardDialog { XamlRoot = Views.ShellPage._XamlRoot }.ShowAsync();

    [RelayCommand]
    public Task Refresh() => Task.Run(_authenticationService.RefreshCurrentAccount).ContinueWith(task =>
    {
        if (task.IsFaulted)
            _notificationService.NotifyException("_AccountRefreshFailedTitle", task.Exception, "_AccountRefreshFailedDescription");
        else _notificationService.NotifyMessage(
            ResourceUtils.GetValue("Notifications", "_AccountRefreshedTitle"),
            ResourceUtils.GetValue("Notifications", "_AccountRefreshedDescription").Replace("${name}", _accountService.ActiveAccount.Name));
    });

    [RelayCommand]
    public void Switch()
    {
        var switchAccountDialog = new SwitchAccountDialog
        {
            XamlRoot = Views.ShellPage._XamlRoot,
            DataContext = App.Services.GetService<SwitchAccountDialogViewModel>()
        };
        _ = switchAccountDialog.ShowAsync();
    }
}