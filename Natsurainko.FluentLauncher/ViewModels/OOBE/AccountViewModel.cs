using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

internal partial class AccountViewModel : ObservableRecipient, IRecipient<ActiveAccountChangedMessage>
{
    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    [ObservableProperty]
    private Account activeAccount;

    private readonly AccountService _accountService;

    public AccountViewModel(AccountService accountService)
    {
        _accountService = accountService;
        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
        {
            CanNext = ActiveAccount is not null,
            NextPage = typeof(Views.OOBE.GetStartedPage)
        });
        IsActive = true;
    }

    bool processingActiveAccountChangedMessage = false;

    public void Receive(ActiveAccountChangedMessage message)
    {
        processingActiveAccountChangedMessage = true;
        ActiveAccount = message.Value;
        processingActiveAccountChangedMessage = false;
    }

    partial void OnActiveAccountChanged(Account value)
    {
        WeakReferenceMessenger.Default.Send(new GuideNavigationMessage()
        {
            CanNext = value is not null,
            NextPage = typeof(Views.OOBE.GetStartedPage)
        });

        if (!processingActiveAccountChangedMessage)
            _accountService.Activate(value);
    }

    [RelayCommand]
    public void Login(Button parameter) => _ = new AuthenticationWizardDialog { XamlRoot = parameter.XamlRoot }.ShowAsync();
}