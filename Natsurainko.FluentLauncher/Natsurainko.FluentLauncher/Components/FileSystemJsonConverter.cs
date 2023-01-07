using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components;

public class FileInfoJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(FileInfo);

    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jValue = serializer.Deserialize<JValue>(reader);
        if (jValue.Value == null)
            return null;

        return new FileInfo(jValue.ToString());
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var file = value as FileInfo;
        writer.WriteValue(file.FullName);
    }
}

public class DirectoryInfoJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(DirectoryInfo);

    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jValue = serializer.Deserialize<JValue>(reader);
        if (jValue.Value == null)
            return null;

        return new DirectoryInfo(jValue.ToString());
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var file = value as DirectoryInfo;
        writer.WriteValue(file.FullName);
    }
}
