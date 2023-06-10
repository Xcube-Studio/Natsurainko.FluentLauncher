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
using System.ComponentModel;

namespace Natsurainko.FluentLauncher.Services.Accounts;

class AccountService
{
    public ReadOnlyObservableCollection<IAccount> Accounts { get; }
    private readonly ObservableCollection<IAccount> _accounts = new();

    public event NotifyCollectionChangedEventHandler AccountsChanged
    {
        add => _accounts.CollectionChanged += value;
        remove => _accounts.CollectionChanged -= value;
    }

    public IAccount ActiveAccount { get; private set; }

    public event PropertyChangedEventHandler ActiveAccountChanged;


    public AccountService()
    {
        Accounts = new ReadOnlyObservableCollection<IAccount>(_accounts);
    }

    /// <summary>
    /// Set an account as the active account
    /// </summary>
    /// <param name="account">The account to be activated</param>
    /// <exception cref="ArgumentException">Thrown when the account provided is not managed by the AccountService</exception>
    public void Activate(IAccount account)
    {
        if (_accounts.Contains(account))
            ActiveAccount = account;
        else
            throw new ArgumentException($"{account} is not an account managed by AccountService", nameof(account));
    }

    /// <summary>
    /// Remove an account managed by AccountService
    /// </summary>
    /// <param name="account">Account to be removed</param>
    /// <returns>Returns true if successful</returns>
    public bool Remove(IAccount account)
    {
        return _accounts.Remove(account);
    }

    /// <summary>
    /// Add an account to the AccountService by authentication
    /// </summary>
    /// <param name="accountType">Type of the account to be added</param>
    /// <param name="param">Authentication parameters</param>
    /// <returns>Returns true if successful</returns>
    public bool AddAccount(AccountType accountType, object param)
    {
        throw new NotImplementedException();
    }
}
