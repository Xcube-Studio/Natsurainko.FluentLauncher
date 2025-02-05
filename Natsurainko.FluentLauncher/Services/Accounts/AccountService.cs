using CommunityToolkit.Mvvm.Messaging;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Nrk.FluentCore.Authentication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Accounts;

internal class AccountService
{
    // Dependencies
    private readonly LocalStorageService _storageService;
    private readonly SettingsService _settingsService;
    private readonly AuthenticationService _authService;

    public readonly string AccountsJsonPath = Path.Combine("settings", "accounts.json");

    #region Accounts

    public ReadOnlyObservableCollection<Account> Accounts { get; }
    private readonly ObservableCollection<Account> _accounts;

    public event NotifyCollectionChangedEventHandler AccountsChanged
    {
        add => _accounts.CollectionChanged += value;
        remove => _accounts.CollectionChanged -= value;
    }

    #endregion

    #region ActiveAccount

    private Account? _activeAccount;
    public Account? ActiveAccount
    {
        get => _activeAccount;
        set
        {
            if (_activeAccount != value)
            {
                _settingsService.ActiveAccountUuid = value?.Uuid;
                ActiveAccountChanged?.Invoke(this, value);
            }
            _activeAccount = value;
        }
    }

    public event EventHandler<Account?>? ActiveAccountChanged;

    #endregion

    public AccountService(
        SettingsService settingsService,
        LocalStorageService storageService,
        AuthenticationService authService)
    {
        _settingsService = settingsService;
        _storageService = storageService;
        _authService = authService;

        _accounts = new ObservableCollection<Account>(InitializeAccountCollection());
        Accounts = new ReadOnlyObservableCollection<Account>(_accounts);

        if (_settingsService.ActiveAccountUuid is Guid uuid)
            ActivateAccount(_accounts.Where(x => x.Uuid == uuid).FirstOrDefault());

        _accounts.CollectionChanged += (_, e) => SaveData();

        WeakReferenceMessenger.Default.Register<AccountAddedMessage>(this, (r, m) => App.DispatcherQueue.TryEnqueue(() => _accounts.Add(m.Value)));
        WeakReferenceMessenger.Default.Register<AccountRemovedMessage>(this, (r, m) => App.DispatcherQueue.TryEnqueue(() => _accounts.Remove(m.Value)));
    }

    /// <summary>
    /// Loads the account list to Accounts from a JSON file
    /// </summary>
    private IEnumerable<Account> InitializeAccountCollection()
    {
        // Read settings/accounts.json from local storage service
        var accountJson = App.GetService<LocalStorageService>().GetFile(AccountsJsonPath);
        string accountsJson = accountJson.Exists ? File.ReadAllText(accountJson.FullName) : "[]";

        // Parse accounts.json
        if (JsonNode.Parse(accountsJson) is not JsonNode jsonNode)
            yield break;

        foreach (var item in jsonNode.AsArray())
        {
            var accountType = (AccountType)(item?["Type"]?.GetValue<int>()).GetValueOrDefault(0);

            Account? account = accountType switch
            {
                AccountType.Offline => item.Deserialize(FLSerializerContext.Default.OfflineAccount),
                AccountType.Microsoft => item.Deserialize(FLSerializerContext.Default.MicrosoftAccount),
                AccountType.Yggdrasil => item.Deserialize(FLSerializerContext.Default.YggdrasilAccount),
                _ => null
            };

            if (account is null)
                continue;

            yield return account;
        }
    }

    /// <summary>
    /// Save the account list to a JSON file
    /// Called automatically when Accounts is changed
    /// </summary>
    private void SaveData()
    {
        // Save to file
        string json = JsonSerializer.Serialize(Accounts.ToArray(), FLSerializerContext.Default.AccountArray);
        var file = _storageService.GetFile(AccountsJsonPath);

        if (file.Directory != null && !file.Directory.Exists)
            file.Directory.Create();

        File.WriteAllText(file.FullName, json);
    }

    public void AddAccount(Account account)
    {
        if (Accounts.Where(x => x.Type.Equals(account.Type) && x.Uuid.Equals(account.Uuid) && x.Name.Equals(account.Name)).Any())
            throw new Exception("There cannot be two accounts with the same account type and name and UUID");

        // _accounts.Add(account);
        WeakReferenceMessenger.Default.Send(new AccountAddedMessage(account));
    }

    public void RemoveAccount(Account account, bool dontActive = false)
    {
        // bool result = _accounts.Remove(account);
        WeakReferenceMessenger.Default.Send(new AccountRemovedMessage(account));

        if (ActiveAccount == account && !dontActive)
            this.ActivateAccount(_accounts.Count != 0 ? _accounts[0] : null);

        // return result;
    }

    public void ActivateAccount(Account? account)
    {
        if (account != null && !_accounts.Contains(account))
            throw new ArgumentException($"{account} is not an account managed by AccountService", nameof(account));

        ActiveAccount = account;
    }

    public async Task<Account> RefreshAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        // RefreshAsync account
        Account refreshedAccount = account switch
        {
            MicrosoftAccount microsoftAccount => await _authService.RefreshAsync(microsoftAccount, cancellationToken),
            YggdrasilAccount yggdrasilAccount => await _authService.RefreshAsync(yggdrasilAccount, cancellationToken),
            OfflineAccount offlineAccount => _authService.Refresh(offlineAccount),

            _ => throw new InvalidOperationException("Unknown account type")
        };

        // Update stored account
        Account? oldAccount = Accounts.FirstOrDefault(x => x.ProfileEquals(account)) 
            ?? throw new Exception($"{account} does not exist in AccountService");

        bool isActiveAccount = ActiveAccount == oldAccount;
        RemoveAccount(oldAccount, true);
        AddAccount(refreshedAccount);

        if (isActiveAccount)
            ActivateAccount(refreshedAccount);

        _ = App.GetService<CacheSkinService>().CacheSkinOfAccount(refreshedAccount);

        return refreshedAccount;
    }
}
