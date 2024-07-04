using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppSettingsManagement.Converters;

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


    /// <summary>
    /// Returns the type used in the application
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public T? Convert(string json) => (T?)((IDataTypeConverter)this).Convert(json);
    
    /// <inheritdoc/>
    object? IDataTypeConverter.Convert(object? source)
    {
        if (source is not string json)
            return null;

        return JsonSerializer.Deserialize(json, TargetType);
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

        return JsonSerializer.Serialize(target, TargetType);
    }
}
