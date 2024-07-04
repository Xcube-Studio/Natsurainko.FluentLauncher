using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSettingsManagement.Converters;

/// <summary>
/// ConvertFrom is used when converting an object from the displayed type to the stored type. <br/>
/// Convert is used when converting an object from the stored type to the displayed type.
/// </summary>
public interface IDataTypeConverter
{
    /// <summary>
    /// Type stored in the storage
    /// </summary>
    Type SourceType { get; }
    /// <summary>
    /// Type used in the application
    /// </summary>
    Type TargetType { get; }

    object? Convert(object? source);
    object? ConvertFrom(object? target);
}

public static class DataTypeConverters
{
    public static Dictionary<Type, IDataTypeConverter> Converters { get; } = new();

    /// <summary>
    /// Returns an instance of the converter for the specified type.
    /// </summary>
    /// <param name="type">Type of the converter class</param>
    /// <remarks>
    /// Singletons of the converters are stored in the Converters dictionary.
    /// </remarks>
    public static IDataTypeConverter GetConverter(Type type)
    {
        // Checks if type is IDataTypeConverter
        if (typeof(IDataTypeConverter).IsAssignableFrom(type))
        {
            // Checks if the converter is already registered
            if (!Converters.ContainsKey(type))
            {
                // Creates an instance of the converter
                var converter = (IDataTypeConverter)Activator.CreateInstance(type)!;
                Converters.Add(type, converter);
            }
            return Converters[type];
        }
        else
        {
            throw new ArgumentException("The specified type is not a data type converter.");
        }
    }
}
