using System;
using System.Collections.Generic;
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
public class JsonStringConverter<T> : IDataTypeConverter
{
    /// <inheritdoc/>
    public Type SourceType => typeof(string);

    /// <inheritdoc/>
    public Type TargetType => typeof(T);

    private readonly JsonSerializerContext _serializerContext;

    public JsonStringConverter()
    {
        if (JsonStringConverterConfig.SerializerContext is null)
            throw new InvalidOperationException("JsonSerializerContext is not available.");
        _serializerContext = JsonStringConverterConfig.SerializerContext;
    }

    /// <summary>
    /// Returns the type used in the application
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public T? Convert(string json) => (T?)((IDataTypeConverter)this).Convert(json);
    
    /// <inheritdoc/>
    object? IDataTypeConverter.Convert(object? source)
    {
        if (source is not string json || JsonStringConverterConfig.SerializerContext is null)
            return null;

        return JsonSerializer.Deserialize(json, TargetType, _serializerContext);
    }

    /// <summary>
    /// Returns the json string of an object
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public string? Convert(T target) => (string?)((IDataTypeConverter)this).ConvertFrom(target);

    /// <inheritdoc/>
    object? IDataTypeConverter.ConvertFrom(object? target)
    {
        if (target is not T)
            return null;

        return JsonSerializer.Serialize(target, TargetType, _serializerContext);
    }
}

public static class JsonStringConverterConfig
{
    public static JsonSerializerContext? SerializerContext { get; set; }
}
