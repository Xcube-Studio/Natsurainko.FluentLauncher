using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppSettingsManagement.Converters;

namespace AppSettingsManagement;

public abstract class SettingsContainer : ISettingsContainer
{
    public static readonly string ROOT_CONTAINER_NAME = "_ROOT_";

    #region ISettingsContainer Members

    public event SettingChangedEventHandler? SettingsChanged;

    public ISettingsStorage Storage { get; init; }

    public string Name { get; init; }

    public ISettingsContainer? Parent { get; init; }

    #endregion ISettingsContainer Members


    /// <summary>
    /// Used to initialize the root container
    /// </summary>
    /// <param name="storage"></param>
    public SettingsContainer(ISettingsStorage storage)
    {
        Storage = storage;
        Name = ROOT_CONTAINER_NAME;
        InitializeContainers();
    }

    public SettingsContainer(ISettingsStorage storage, string name, ISettingsContainer parent)
    {
        Storage = storage;
        Name = name;
        Parent = parent;
        InitializeContainers();
    }

    /// <summary>
    /// Initialize child containers
    /// </summary>
    protected virtual void InitializeContainers() { }


    /// <summary>
    /// Path to access the item in the hierarchical storage
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string GetPathFromKey(string key)
    {
        Stack<string> levels = new();
        ISettingsContainer container = this;
        while (container.Parent is not null)
        {
            levels.Push(container.Name);
            container = container.Parent;
        }

        StringBuilder sb = new();
        foreach (string item in levels)
        {
            sb.Append($"{item}/");
        }
        sb.Append(key);
        return sb.ToString();
    }

    #region GetValue<T>

    protected T? GetValue<T>(string key, IDataTypeConverter? converter = null)
    {
        var path = GetPathFromKey(key);

        if (!Storage.Contains(path))
        {
            return default(T?); // null
        }

        Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        // If a type converter is given, use it to convert the value; otherwise, return the value as the type stored
        if (converter is not null)
        {
            if (converter.TargetType == type)
            {
                object value = Storage.GetValue(path, converter.SourceType);
                if (value.GetType() == converter.SourceType)
                    return (T?)converter.Convert(value);
            }

            throw new ArgumentException($"Typer converter given cannot convert to {type}", nameof(converter));
        }
        else
        {
            return (T)Storage.GetValue(path, type);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue">Cannot be null; the setting item is removed if set to null.</param>
    /// <returns></returns>
    protected T GetValue<T>(string key, T defaultValue, IDataTypeConverter? converter = null) where T : notnull
    {
        if (defaultValue is null)
            throw new ArgumentNullException(nameof(defaultValue));

        if (converter is not null && converter.TargetType != typeof(T))
            throw new ArgumentException($"Type converter given cannot convert to {typeof(T)}", nameof(converter));

        var path = GetPathFromKey(key);

        object defaultValueConverted = converter?.ConvertFrom(defaultValue) ?? defaultValue;

        if (!Storage.Contains(path)) // key is not found
        {
            Storage.SetValue(path, defaultValueConverted, defaultValueConverted.GetType());
            return defaultValue;
        }

        // Try to get the value from the storage
        T? value = GetValue<T>(key, converter);
        if (value is null) 
        {
            Storage.SetValue(path, defaultValueConverted, defaultValueConverted.GetType());
            return defaultValue;
        }

        return value;
    }

    #endregion

    #region SetValue<T>

    private void _setValue<T>(string key,
                              T value,
                              SettingChangedEventHandler? _event,
                              IDataTypeConverter? converter = null) where T : notnull
    {
        var path = GetPathFromKey(key);
        object? currentValue;
        if (converter is not null)
        {
            if (typeof(T) != converter.TargetType)
                throw new ArgumentException($"Type converter given cannot convert from {typeof(T)}", nameof(converter));
            currentValue = Storage.Contains(path) ? Storage.GetValue(path, converter.SourceType) : null;
        }
        else
        {
            currentValue = Storage.Contains(path) ? Storage.GetValue<T>(path) : null;
        }

        if (value.Equals(currentValue))
            return;

        // Only set the value when the new value is different
        if (converter is not null) // Convert value from T using the converter
        {
            if (typeof(T) != converter.TargetType)
                throw new ArgumentException($"Type converter given cannot convert from {typeof(T)}", nameof(converter));

            object? convertedValue = converter.ConvertFrom(value);

            if(convertedValue is null)
                Storage.DeleteItem(path);
            else
                Storage.SetValue(path, convertedValue);
        }
        else // No converter is given; store the value as type T.
        {
            Storage.SetValue<T>(path, value);
        }

        // Notify a setting item has been changed
        _event?.Invoke(this, new SettingChangedEventArgs(path, value));
        SettingsChanged?.Invoke(this, new SettingChangedEventArgs(path, value));
    }

    /// <summary>
    /// Wrap SetValue for value types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="_event"></param>
    protected void SetValue<T>(string key,
                               T? value,
                               SettingChangedEventHandler? _event,
                               IDataTypeConverter? converter = null) where T : struct
    {
        if (value is null)
        {
            Storage.DeleteItem(key);
            return;
        }
        _setValue<T>(key, (T)value, _event, converter);
    }

    /// <summary>
    /// Wrap SetValue for reference types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="_event"></param>
    protected void SetValue<T>(string key,
                               T? value,
                               SettingChangedEventHandler? _event,
                               IDataTypeConverter? converter = null) where T : class
    {
        if (value is null)
        {
            Storage.DeleteItem(key);
            return;
        }
        _setValue<T>(key, value, _event, converter);
    }

    #endregion SetValue<T>
}