using Newtonsoft.Json;

/* 项目“Natsurainko.FluentLauncher (SelfContained)”的未合并的更改
在此之前:
using Newtonsoft.Json;
在此之后:
using Newtonsoft.Json.Linq;
*/
using Newtonsoft.Json.Linq;
using System;
using System.IO;

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
