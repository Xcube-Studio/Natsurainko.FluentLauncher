using Newtonsoft.Json;
using System;
using System.IO;

namespace Natsurainko.Toolkits.Text
{
    public interface IJsonEntity
    {
    }

    public static class JsonEntityExtension
    {
        public static JsonSerializerSettings SerializerSettings { get; set; }
            = new JsonSerializerSettings() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };

        public static string ToJson<T>(this T entity) where T : IJsonEntity => JsonConvert.SerializeObject(entity, SerializerSettings);

        public static string ToJson(this object entity) => JsonConvert.SerializeObject(entity, SerializerSettings);

        public static T FromJson<T>(this T entity, string json) where T : IJsonEntity => JsonConvert.DeserializeObject<T>(json);

        public static T ToJsonEntity<T>(this string json) => JsonConvert.DeserializeObject<T>(json);

        public static void WriteAllText<T>(this T entity, Uri uri) where T : IJsonEntity => File.WriteAllText(uri.AbsoluteUri, entity.ToJson());
    }
}
