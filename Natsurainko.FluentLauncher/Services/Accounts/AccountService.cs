using Natsurainko.FluentCore.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Natsurainko.FluentCore.Model.Auth;
using System.Collections.Specialized;

namespace Natsurainko.FluentLauncher.Services.Accounts;

class AccountService
{
    public ReadOnlyCollection<IAccount> Accounts { get; }
    public IAccount ActiveAccount { get; private set; }

    private readonly ObservableCollection<IAccount> _accounts = new();

    public event NotifyCollectionChangedEventHandler AccountsChanged
    {
        add => _accounts.CollectionChanged += value;
        remove => _accounts.CollectionChanged -= value;
    }

    public AccountService()
    {
        
        Accounts = _accounts.AsReadOnly();
    }


    public void Activate(IAccount account)
    {
        if (_accounts.Contains(account))
            ActiveAccount = account;
        else
            throw new ArgumentException($"{account} is not an account managed by AccountService", nameof(account));
    }

    public bool Remove(IAccount account)
    {
        return _accounts.Remove(account);
    }

    public bool AddAccount(AccountType accountType, object param)
    {
        throw new NotImplementedException();
    }
}
