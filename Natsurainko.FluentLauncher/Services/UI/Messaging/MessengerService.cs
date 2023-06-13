using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Services.Accounts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

/// <summary>
/// A singleton service for managing events subscription of other components and global messaging
/// </summary>
class MessengerService
{
    private readonly AccountService _accountService;

    public MessengerService(AccountService accountService)
    {
        _accountService = accountService;
    }

    public void SubscribeEvents()
    {
        _accountService.ActiveAccountChanged += AccountService_ActiveAccountChanged;
    }

    private void AccountService_ActiveAccountChanged(object sender, PropertyChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new ActiveAccountChangedMessage(_accountService.ActiveAccount));
    }
}
