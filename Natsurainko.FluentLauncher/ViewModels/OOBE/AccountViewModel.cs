using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.ViewModels.Home;
using Natsurainko.FluentLauncher.Views.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

partial class AccountViewModel : ObservableRecipient, IRecipient<ActiveAccountChangedMessage>
{
    public ReadOnlyObservableCollection<IAccount> Accounts { get; init; }

    [ObservableProperty]
    private IAccount activeAccount;

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

    partial void OnActiveAccountChanged(IAccount value)
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
    public async void Login(Button parameter) 
    {
        await new AuthenticationWizardDialog { XamlRoot = parameter.XamlRoot }.ShowAsync();
    }

    private void SetAccount(IAccount account) => App.MainWindow.DispatcherQueue.TryEnqueue(() =>
    {
#pragma warning disable CS0612 // Type or member is obsolete
        _accountService.AddAccount(account);
#pragma warning restore CS0612 // Type or member is obsolete
        _accountService.Activate(account);

        MessageService.ShowSuccess($"Add {account.Type} Account Successfully", $"Welcome back, {account.Name}");
    });
}