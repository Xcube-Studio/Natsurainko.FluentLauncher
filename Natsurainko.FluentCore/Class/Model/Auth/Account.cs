using Natsurainko.Toolkits.Values;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Natsurainko.FluentCore.Class.Model.Auth;

[JsonConverter(typeof(AccountJsonConverter))]
public abstract class Account
{
    public string Name { get; set; }

    public Guid Uuid { get; set; }

    public string AccessToken { get; set; }

    public string ClientToken { get; set; }

    public virtual AccountType Type { get; set; } = AccountType.Offline;

    public static OfflineAccount Default { get; private set; } = new()
    {
        Name = "Steve",
        Uuid = GuidHelper.FromString("Steve"),
        AccessToken = Guid.NewGuid().ToString("N"),
        ClientToken = Guid.NewGuid().ToString("N")
    };
}

public class YggdrasilAccount : Account
{
    public override AccountType Type => AccountType.Yggdrasil;

    public string YggdrasilServerUrl { get; set; }
}

public class MicrosoftAccount : Account
{
    public override AccountType Type => AccountType.Microsoft;

    public string RefreshToken { get; set; }

    public DateTime DateTime { get; set; }
}

public class OfflineAccount : Account
{
    public override AccountType Type => AccountType.Offline;
}

public class AccountJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Account);

    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jobject = serializer.Deserialize<JObject>(reader);

        if (jobject == null)
            return null;

        var accountType = (AccountType)jobject["Type"].Value<int>();

        return accountType switch
        {
            AccountType.Offline => new OfflineAccount
            {
                AccessToken = jobject["AccessToken"].ToObject<string>(),
                ClientToken = jobject["ClientToken"].ToObject<string>(),
                Name = jobject["Name"].ToObject<string>(),
                Uuid = jobject["Uuid"].ToObject<Guid>()
            },
            AccountType.Microsoft => new MicrosoftAccount
            {
                AccessToken = jobject["AccessToken"].ToObject<string>(),
                ClientToken = jobject["ClientToken"].ToObject<string>(),
                Name = jobject["Name"].ToObject<string>(),
                Uuid = jobject["Uuid"].ToObject<Guid>(),
                DateTime = jobject["DateTime"].ToObject<DateTime>(),
                RefreshToken = jobject["RefreshToken"].ToObject<string>()
            },
            AccountType.Yggdrasil => new YggdrasilAccount
            {
                AccessToken = jobject["AccessToken"].ToObject<string>(),
                ClientToken = jobject["ClientToken"].ToObject<string>(),
                Name = jobject["Name"].ToObject<string>(),
                Uuid = jobject["Uuid"].ToObject<Guid>(),
                YggdrasilServerUrl = jobject["YggdrasilServerUrl"].ToObject<string>()
            },
            _ => null
        };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
}
