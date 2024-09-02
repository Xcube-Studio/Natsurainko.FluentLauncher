﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.Settings.Converters;

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
    private static Dictionary<Type, IDataTypeConverter> _converters { get; } = new();

    /// <summary>
    /// Returns an instance of the converter for the specified type.
    /// </summary>
    /// <param name="type">Type of the converter class</param>
    /// <remarks>
    /// Singletons of the converters are stored in the Converters dictionary.
    /// </remarks>
    public static IDataTypeConverter GetConverter([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type)
    {
        // Checks if type is IDataTypeConverter
        if (typeof(IDataTypeConverter).IsAssignableFrom(type))
        {
            // Checks if the converter is already registered
            if (!_converters.ContainsKey(type))
            {
                // Creates an instance of the converter
                var converter = (IDataTypeConverter)Activator.CreateInstance(type)!;
                _converters.Add(type, converter);
            }
            return _converters[type];
        }
        else
        {
            throw new ArgumentException("The specified type is not a data type converter.");
        }
    }
}
