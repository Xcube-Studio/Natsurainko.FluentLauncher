using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Shared.Desktop;
using Newtonsoft.Json;
using System;

namespace Natsurainko.FluentLauncher.Shared.Class;

internal class GameCoreJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(GameCore);

    public override bool CanRead => false;

    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var gameCore = value as GameCore;
        writer.WriteRawValue(JsonConvert.SerializeObject(new
        {
            gameCore.HasModLoader,
            gameCore.Id,
            gameCore.JavaVersion,
            gameCore.MainClass,
            gameCore.Root,
            gameCore.Source,
            gameCore.Type
        }, MethodRequestBuilder.JsonConverters));
    }
}
