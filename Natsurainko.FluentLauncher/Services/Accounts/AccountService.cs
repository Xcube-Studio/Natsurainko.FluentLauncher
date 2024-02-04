using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Services.Accounts;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Natsurainko.FluentLauncher.Services.Accounts;

internal class AccountService : DefaultAccountService
{
    public event NotifyCollectionChangedEventHandler AccountsChanged
    {
        add => _accounts.CollectionChanged += value;
        remove => _accounts.CollectionChanged -= value;
    }
    public event EventHandler<Account?>? ActiveAccountChanged;

    public readonly string AccountsJsonPath = Path.Combine("settings", "accounts.json");

    private readonly LocalStorageService _storageService;
    private readonly SettingsService _settingsService;

    public AccountService(SettingsService settingsService, LocalStorageService storageService)
        : base()
    {
        _settingsService = settingsService;
        _storageService = storageService;

        if (_settingsService.ActiveAccountUuid is Guid uuid)
            this.ActivateAccount(_accounts.Where(x => x.Uuid == uuid).FirstOrDefault());

        _accounts.CollectionChanged += (_, e) => SaveData();
    }

    /// <summary>
    /// Loads the account list to Accounts from a JSON file
    /// </summary>
    public override IEnumerable<Account> InitializeAccountCollection()
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

    public override void WhenActiveAccountChanged(Account? oldAccount, Account? newAccount)
    { 
        _settingsService.ActiveAccountUuid = newAccount?.Uuid;
        ActiveAccountChanged?.Invoke(this, newAccount);
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
}
