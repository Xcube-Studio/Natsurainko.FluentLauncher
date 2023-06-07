using AppSettingsManagement.Converters;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Settings;

class AccountToJsonConverter : IDataTypeConverter
{
    public Type SourceType => typeof(string); // Can

    public Type TargetType => typeof(IAccount); // Can be null

    public object Convert(object source)
    {
        if (source is not string json || json == "null")
            return null;

        var jsonNode = JsonNode.Parse(json);
        var accountType = (AccountType)jsonNode["Type"].GetValue<int>();

        IAccount account = accountType switch
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
