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
using Natsurainko.FluentLauncher.Services.Storage;
using System.IO;
using System.Text.Json.Nodes;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Services.Accounts;

class AccountService
{
    #region Accounts

    public ReadOnlyObservableCollection<IAccount> Accounts { get; }

    private readonly ObservableCollection<IAccount> _accounts = new();

    public event NotifyCollectionChangedEventHandler AccountsChanged
    {
        add => _accounts.CollectionChanged += value;
        remove => _accounts.CollectionChanged -= value;
    }

    #endregion

    #region ActiveAccount

    /// <summary>
    /// The active account of Fluent Launcher. The Accounts collection always contains this account. This is null if no account is available.
    /// </summary>
    public IAccount ActiveAccount { get; private set; }

    public event PropertyChangedEventHandler ActiveAccountChanged;

    #endregion

    public readonly string AccountsJsonPath = Path.Combine("settings", "accounts.json");

    private readonly LocalStorageService _storageService;
    private readonly SettingsService _settingsService;

    public AccountService(LocalStorageService storageService, SettingsService settingsService)
    {
        _storageService = storageService;
        _settingsService = settingsService;

        LoadAccountsList();
        if (_settingsService.ActiveAccountUuid is Guid uuid)
        {
            IAccount activeAccount = _accounts.Where(x => x.Uuid == uuid).FirstOrDefault();
            if (activeAccount is not null)
                ActiveAccount = activeAccount;
            else
                _settingsService.ActiveAccountUuid = null; // TODO: Prompt the user that there is an error
        }

        _accounts.CollectionChanged += (_, e) => SaveData(); // Save the account list when the collection is changed
        Accounts = new ReadOnlyObservableCollection<IAccount>(_accounts);
    }

    /// <summary>
    /// Set an account as the active account
    /// </summary>
    /// <param name="account">The account to be activated</param>
    /// <exception cref="ArgumentException">Thrown when the account provided is not managed by the AccountService</exception>
    public void Activate(IAccount account)
    {
        if (!_accounts.Contains(account))
            throw new ArgumentException($"{account} is not an account managed by AccountService", nameof(account));

        if (ActiveAccount != account)
        {
            ActiveAccount = account;
            _settingsService.ActiveAccountUuid = account.Uuid;
            ActiveAccountChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveAccount)));
        }
    }

    /// <summary>
    /// Remove an account managed by AccountService
    /// </summary>
    /// <param name="account">Account to be removed</param>
    /// <returns>Returns true if successful</returns>
    public bool Remove(IAccount account)
    {
        bool result = _accounts.Remove(account);

        // If the account removed is the active account, activate the first account available.
        // If no more account is available, set to null.
        if (ActiveAccount == account)
        {
            if (_accounts.Count != 0)
            {
                Activate(_accounts[0]);
            }
            else
            {
                ActiveAccount = null;
                _settingsService.ActiveAccountUuid = null;
                ActiveAccountChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ActiveAccount)));
            }
        }

        return result;
    }

    [Obsolete]
    public void AddAccount(IAccount account)
    {
        _accounts.Add(account);
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

    /// <summary>
    /// Loads the account list to Accounts from a JSON file
    /// </summary>
    private void LoadAccountsList()
    {
        // Read settings/accounts.json from local storage service
        string accountJsonPath = _storageService.GetFile(AccountsJsonPath).FullName;
        string accountsJson = File.ReadAllText(accountJsonPath);

        // Parse accounts.json
        if (JsonNode.Parse(accountsJson) is not JsonNode jsonNode)
            return;

        foreach (var item in jsonNode.AsArray())
        {
            var accountType = (AccountType)(item?["Type"].GetValue<int>());
            IAccount account = accountType switch
            {
                AccountType.Offline => item.Deserialize<OfflineAccount>(),
                AccountType.Microsoft => item.Deserialize<MicrosoftAccount>(),
                AccountType.Yggdrasil => item.Deserialize<YggdrasilAccount>(),
                _ => null
            };

            if (account is null)
                continue;

            _accounts.Add(account); // At this point, CollectionChanged handler has not been registered.
        }
    }

    /// <summary>
    /// Save the account list to a JSON file
    /// Called automatically when Accounts is changed
    /// </summary>
    private void SaveData()
    {
        var jsonArray = new JsonArray();
        foreach (var item in Accounts)
        {
            // Use derived types to store all properties
            if (item is OfflineAccount offlineAccount)
                jsonArray.Add(offlineAccount);
            else if (item is MicrosoftAccount microsoftAccount)
                jsonArray.Add(microsoftAccount);
            else if ((item is YggdrasilAccount yggdrasilAccount))
                jsonArray.Add(yggdrasilAccount);
        }

        // Save to file
        string json = jsonArray.ToJsonString();
        string path = _storageService.GetFile(AccountsJsonPath).FullName;
        File.WriteAllText(path, json);
    }
}
