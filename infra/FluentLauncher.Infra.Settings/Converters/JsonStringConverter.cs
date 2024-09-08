using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.Settings.Converters;

/// <summary>
/// Converts a string to a type T using JSON serialization
/// </summary>
/// <typeparam name="T">Type used in the application</typeparam>
public class JsonStringConverter<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> : IDataTypeConverter<string, T?>
{
    private readonly JsonSerializerContext _serializerContext;

    public JsonStringConverter()
    {
        if (JsonStringConverterConfig.SerializerContext is null)
            throw new InvalidOperationException("JsonSerializerContext is not available.");
        _serializerContext = JsonStringConverterConfig.SerializerContext;
    }

    public static IDataTypeConverter<string, T?> Instance { get; } = new JsonStringConverter<T>();

    /// <summary>
    /// Returns the type used in the application
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public T? Convert(string json)
    {
        if (json is null || JsonStringConverterConfig.SerializerContext is null)
            return default;

        return (T?)JsonSerializer.Deserialize(json, typeof(T), _serializerContext);
    }

    /// <summary>
    /// Returns the json string of an object
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public string ConvertFrom(T? target)
    {
        return JsonSerializer.Serialize(target, typeof(T), _serializerContext);
    }
}

public static class JsonStringConverterConfig
{
    public static JsonSerializerContext? SerializerContext { get; set; }
}
