using Natsurainko.FluentLauncher.Shared.Class;
using Newtonsoft.Json;
using System;

namespace Natsurainko.FluentLauncher.Shared.Desktop;

public class MethodRequestBuilder
{
    public static JsonConverter[] JsonConverters { get; private set; } = new JsonConverter[]
    {
        new GameCoreJsonConverter(),
        new FileInfoJsonConverter(),
        new DirectoryInfoJsonConverter()
    };

    private MethodRequest MethodRequest = new() { ImplementId = Guid.NewGuid() };

    public MethodRequestBuilder SetMethod(string method)
    {
        MethodRequest.Method = method;
        return this;
    }

    public MethodRequestBuilder AddParameter(object parameter)
    {
        MethodRequest.MethodParameters.Add(new MethodParameter { Content = JsonConvert.SerializeObject(parameter, JsonConverters), Type = parameter.GetType().FullName });
        return this;
    }

    public MethodRequestBuilder AddParameter((object, string) parameter)
    {
        MethodRequest.MethodParameters.Add(new MethodParameter { Content = JsonConvert.SerializeObject(parameter.Item1, JsonConverters), Type = parameter.Item2 });
        return this;
    }

    public MethodRequest Build() => MethodRequest;

    public static MethodRequestBuilder Create() => new();

}
