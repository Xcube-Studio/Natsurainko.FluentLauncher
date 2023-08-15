using AppSettingsManagement.Converters;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Classes.Enums;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Natsurainko.FluentLauncher.Utils;

class AccountToJsonConverter : IDataTypeConverter
{
    public Type SourceType => typeof(string);

    public Type TargetType => typeof(Account);

    public object Convert(object source)
    {
        if (source is not string json || json == "null")
            return null;

        var jsonNode = JsonNode.Parse(json);
        var accountType = (AccountType)jsonNode["Type"].GetValue<int>();

        Account account = accountType switch
        {
            AccountType.Offline => jsonNode.Deserialize<OfflineAccount>(),
            AccountType.Microsoft => jsonNode.Deserialize<MicrosoftAccount>(),
            AccountType.Yggdrasil => jsonNode.Deserialize<YggdrasilAccount>(),
            _ => throw new InvalidCastException($"Cannot cast {source} to IAccount")
        };

        return account;
    }

    public object ConvertFrom(object target) => target switch
    {
        OfflineAccount => JsonSerializer.Serialize((OfflineAccount)target),
        MicrosoftAccount => JsonSerializer.Serialize((MicrosoftAccount)target),
        YggdrasilAccount => JsonSerializer.Serialize((YggdrasilAccount)target),
        _ => "null"
    };
}
