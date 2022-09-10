using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Shared.Desktop;

public class MethodRequest
{
    public Guid ImplementId { get; set; }

    public string Method { get; set; }

    public List<MethodParameter> MethodParameters { get; set; } = new();

    public ValueSet CreateValueSet()
    {
        return new ValueSet
        {
            { "ImplementId" , ImplementId.ToString() },
            { "Method", Method },
            { "MethodParameters", JsonConvert.SerializeObject(MethodParameters, MethodRequestBuilder.JsonConverters)  }
        };
    }

    public static MethodRequest CreateFromValueSet(ValueSet valueSet)
    {
        return new MethodRequest
        {
            ImplementId = Guid.Parse((string)valueSet["ImplementId"]),
            Method = (string)valueSet["Method"],
            MethodParameters = valueSet.ContainsKey("MethodParameters") ? JsonConvert.DeserializeObject<IEnumerable<MethodParameter>>((string)valueSet["MethodParameters"], MethodRequestBuilder.JsonConverters).ToList() : new()
        };
    }
}
