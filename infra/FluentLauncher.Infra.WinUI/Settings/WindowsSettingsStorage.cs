using Windows.Storage;
using System.Linq;
using System.Collections;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using System;
using System.Collections.Generic;

namespace AppSettingsManagement.Windows;

// Consider rename to WinRTSettingsStorage
public class WinRTSettingsStorage : ISettingsStorage
{
    private readonly ApplicationDataContainer _rootContainer;
    private readonly Type[] supportedTypes =
    {
        //https://learn.microsoft.com/en-us/windows/apps/design/app-settings/store-and-retrieve-app-data
        // Int
        typeof(int),
        typeof(sbyte),
        typeof(byte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        // Floating point
        typeof(float),
        typeof(double),
        // Char16 and string
        typeof(char),
        typeof(string),
        // Boolean
        typeof(bool),
        // Date and time
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        // Other
        typeof(Guid),
        typeof(Point),
        typeof(Rect),

        // Enum is not supported by ApplicationDataContainer, but it can be stored as ints
        typeof(Enum)
    };


    public WinRTSettingsStorage()
    {
        _rootContainer = ApplicationData.Current.LocalSettings;
    }

    private bool IsTypeSupported(Type? type)
    {
        if (type is null)
            return false;

        return supportedTypes.Contains(type) || type.IsEnum;
    }

    /// <summary>
    /// Checks if a container exists at the given path, and if so, returns the container and the key.
    /// </summary>
    /// <param name="path">Path of a setting item to be checked</param>
    /// <returns>The container and the key</returns>
    private (ApplicationDataContainer?, string) GetContainerAndKey(string path)
    {
        var parts = path.Split('/');
        var container = _rootContainer;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            if (!container.Containers.ContainsKey(part))
                return (null, "");
            container = container.Containers[part];
        }
        return (container, parts[^1]);
    }

    /// <inheritdoc/>
    public bool Contains(string path)
    {
        var (container, key) = GetContainerAndKey(path);
        return container?.Values.ContainsKey(key) ?? false;
    }

    /// <inheritdoc/>
    public void DeleteItem(string path)
    {
        var (container, key) = GetContainerAndKey(path);
        container?.Values.Remove(key);
    }

    #region GetValue

    // ApplicationDataContainer.Values is IPropertySet, which extends ICollection<KeyValuePair<string, object>>,
    // so there is no need to use generic types.

    /// <inheritdoc/>
    public T GetValue<T>(string path) where T : notnull
        => (T)GetValue(path, typeof(T));

    /// <inheritdoc/>
    public object GetValue(string path, Type type)
    {
        (ApplicationDataContainer? container, string key) = GetContainerAndKey(path);

        if (type.IsArray)
        {
            Type elementType = type.GetElementType()!;

            if (!IsTypeSupported(elementType))
                throw new InvalidOperationException($"Type {elementType} is not supported by {nameof(WinRTSettingsStorage)}");

            // WinRT ApplicationDataContainer cannot store empty arrays.
            if (container is not null && container.Values.ContainsKey(key))
            {
                // If the key exists, return the stored array, which is never empty.
                object value = container.Values[key];

                // Check that the stored value is an array of the correct type.
                if (value.GetType().GetElementType() != elementType)
                    throw new Exception($"Item stored at path\"{path}\" is not an array of type {elementType}");

                return value;
            }
            else // If the array is empty, it is stored as null in the ApplicationDataContainer.
            {
                // Return an empty array of the correct type.
                return Array.CreateInstance(elementType, 0);
            }
        }

        // Access single values stored in ApplicationDataContainer

        // Check the type requested
        if (!IsTypeSupported(type))
            throw new InvalidOperationException($"Type {type} is not supported by {nameof(WinRTSettingsStorage)}");

        if (container is null || !container.Values.ContainsKey(key))
            throw new KeyNotFoundException($"Path {path} not found.");

        // Convert enum integral values to their enum types
        if (type.IsEnum)
        {
            // Retrieve enums as their integral types
            var integralValue = container.Values[key];
            return Enum.ToObject(type, integralValue);
        }

        return container.Values[key];
    }

    #endregion

    #region SetValue

    /// <inheritdoc/>
    public void SetValue<T>(string path, T value) where T : notnull
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        // ApplicationDataContainer.Values is IPropertySet, which extends ICollection<KeyValuePair<string, object>>,
        // so there is no need to use generic types.
        SetValue(path, value, value.GetType());
    }

    public void SetValue(string path, object value, Type type)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        // Create container from path if not exists
        var parts = path.Split('/');
        var container = _rootContainer;
        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            if (!container.Containers.ContainsKey(part))
                container.CreateContainer(part, ApplicationDataCreateDisposition.Always);
            container = container.Containers[part];
        }
        var key = parts[^1];

        // Set the value using `key` in `container`
        if (type.IsEnum)
        {
            // Store enums as their integral types
            var integralValue = Convert.ChangeType(value, Enum.GetUnderlyingType(type));
            container.Values[key] = integralValue;
        }
        else if (type.IsArray)
        {
            // Check if the array element type is supported
            if (!IsTypeSupported(type.GetElementType()))
                throw new InvalidOperationException($"Type {type} is not supported by {nameof(WinRTSettingsStorage)}");

            // If array is empty, remove the item, because empty array cannot be stored in ApplicationDataContainer.
            if (value is Array { Length: 0 })
                container.Values[key] = null;
            else if (value is Array)
                container.Values[key] = value;
        }
        else // Other single values
        {
            // Check if the type of `value` is supported
            if (!IsTypeSupported(type))
                throw new InvalidOperationException($"Type {type} is not supported by {nameof(WinRTSettingsStorage)}");

            container.Values[key] = value;
        }
    }

    #endregion
}
