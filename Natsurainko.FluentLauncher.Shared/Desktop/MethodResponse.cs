using Newtonsoft.Json;
using System;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Shared.Desktop;

public class MethodResponse
{
    public Guid ImplementId { get; set; }

    public string Method { get; set; }

    public virtual object Response { get; set; }

    public ValueSet CreateValueSet()
    {
        return new ValueSet
        {
            { "ImplementId" , ImplementId.ToString() },
            { "Method", Method },
            { "Response", JsonConvert.SerializeObject(Response, MethodRequestBuilder.JsonConverters)  }
        };
    }

    public static MethodResponse CreateFromValueSet(ValueSet valueSet)
    {
        return new MethodResponse
        {
            ImplementId = Guid.Parse((string)valueSet["ImplementId"]),
            Method = (string)valueSet["Method"],
            Response = valueSet.ContainsKey("Response") ? (string)valueSet["Response"] : null
        };
    }

    public static MethodResponse<T> CreateFromValueSet<T>(ValueSet valueSet)
    {
        return new MethodResponse<T>
        {
            ImplementId = Guid.Parse((string)valueSet["ImplementId"]),
            Method = (string)valueSet["Method"],
            Response = valueSet.ContainsKey("Response") ? JsonConvert.DeserializeObject<T>((string)valueSet["Response"], MethodRequestBuilder.JsonConverters) : default
        };
    }
}

public class MethodResponse<T> : MethodResponse
{
    public new T Response { get; set; }
}
