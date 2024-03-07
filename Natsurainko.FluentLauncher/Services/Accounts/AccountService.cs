using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Authentication.Microsoft;
using Nrk.FluentCore.Authentication.Offline;
using Nrk.FluentCore.Authentication.Yggdrasil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Accounts;

internal class AccountService
{
    // Dependencies
    private readonly LocalStorageService _storageService;
    private readonly SettingsService _settingsService;
    private readonly AuthenticationService _authService;
    private readonly SkinCacheService _skinCacheService;

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
        AuthenticationService authService,
        SkinCacheService skinCacheService)
    {
        _settingsService = settingsService;
        _storageService = storageService;
        _authService = authService;
        _skinCacheService = skinCacheService;

        _accounts = new ObservableCollection<Account>(InitializeAccountCollection());
        Accounts = new ReadOnlyObservableCollection<Account>(_accounts);

        if (_settingsService.ActiveAccountUuid is Guid uuid)
            ActivateAccount(_accounts.Where(x => x.Uuid == uuid).FirstOrDefault());

        _accounts.CollectionChanged += (_, e) => SaveData();
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
                AccountType.Offline => item.Deserialize<OfflineAccount>(),
                AccountType.Microsoft => item.Deserialize<MicrosoftAccount>(),
                AccountType.Yggdrasil => item.Deserialize<YggdrasilAccount>(),
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
        var file = _storageService.GetFile(AccountsJsonPath);

        if (file.Directory != null && !file.Directory.Exists)
            file.Directory.Create();

        File.WriteAllText(file.FullName, json);
    }

    public void AddAccount(Account account)
    {
        if (Accounts.Where(x => x.Uuid.Equals(account.Uuid) && x.Type.Equals(account.Type)).Any())
            throw new Exception("不可以存在两个账户类型和 Uuid 均相同的账户");

        _accounts.Add(account);
    }

    public bool RemoveAccount(Account account)
    {
        bool result = _accounts.Remove(account);

        if (ActiveAccount == account)
            this.ActivateAccount(_accounts.Count != 0 ? _accounts[0] : null);

        return result;
    }

    public void ActivateAccount(Account? account)
    {
        if (account != null && !_accounts.Contains(account))
            throw new ArgumentException($"{account} is not an account managed by AccountService", nameof(account));

        ActiveAccount = account;
    }

    public async Task RefreshAccount(Account account)
    {
        // Refresh account
        Account refreshedAccount = account switch
        {
            MicrosoftAccount microsoftAccount => await _authService.Refresh(microsoftAccount),
            YggdrasilAccount yggdrasilAccount => (await _authService.Refresh(yggdrasilAccount))
                .First(acc => acc.Uuid.Equals(account.Uuid)),
            OfflineAccount offlineAccount => _authService.Refresh(offlineAccount),

            _ => throw new InvalidOperationException("Unknown account type")
        };

        // Update stored account
        Account? oldAccount = Accounts.Where(x => x.Uuid.Equals(account.Uuid) && x.Type.Equals(account.Type)).FirstOrDefault();
        if (oldAccount == null)
            throw new Exception($"{account} does not exist in AccountService");

        bool isActiveAccount = ActiveAccount == oldAccount;
        RemoveAccount(oldAccount);
        AddAccount(account);

        if (isActiveAccount)
            ActivateAccount(account);

        // Cache skin
        await Task.Run(() => _skinCacheService.TryCacheSkin(refreshedAccount));
    }

    public async Task RefreshActiveAccount()
    {
        if (ActiveAccount is null)
            return;

        await RefreshAccount(ActiveAccount);
    }

}
