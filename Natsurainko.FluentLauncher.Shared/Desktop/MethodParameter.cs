using Newtonsoft.Json;

namespace Natsurainko.FluentLauncher.Shared.Desktop;

public class MethodParameter
{
    public string Type { get; set; }

    public string Content { get; set; }

    public static T DeserializeObject<T>(string text) => JsonConvert.DeserializeObject<T>(text, MethodRequestBuilder.JsonConverters);

    public object GetValue()
    {
        try { return typeof(MethodParameter).GetMethod("DeserializeObject").MakeGenericMethod(System.Type.GetType(Type)).Invoke(null, new object[] { Content }); } catch { return null; }
    }
}
