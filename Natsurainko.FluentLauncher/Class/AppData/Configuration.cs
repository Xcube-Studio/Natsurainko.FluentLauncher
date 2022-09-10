using Natsurainko.FluentLauncher.Shared.Desktop;
using Newtonsoft.Json;
using System.Linq;
using Windows.Storage;

namespace Natsurainko.FluentLauncher.Class.AppData;

public class Configuration<T> where T : new()
{
    public T Value { get; private set; } = new();

    public ApplicationDataContainer Container => ApplicationData.Current.LocalSettings;

    public Configuration(T Default)
    {
        if (!Container.Values.Any())
            Value = Default;
        else foreach (var item in typeof(T).GetProperties())
                if (item.SetMethod != null && !item.SetMethod.IsStatic)
                    item.SetValue(Value, JsonConvert.DeserializeObject(Container.Values.ContainsKey(item.Name) ? (string)Container.Values[item.Name] : string.Empty, item.PropertyType, MethodRequestBuilder.JsonConverters));

        Save();
    }

    public void Save()
    {
        foreach (var item in typeof(T).GetProperties())
            Container.Values[item.Name] = JsonConvert.SerializeObject(item.GetValue(Value), MethodRequestBuilder.JsonConverters);
    }
}
