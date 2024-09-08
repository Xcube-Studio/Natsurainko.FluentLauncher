using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentLauncher.Infra.Settings.Converters;

namespace FluentLauncher.Infra.Settings;

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

    protected T? GetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string key)
        where T : notnull
    {
        var path = GetPathFromKey(key);

        if (!Storage.Contains(path))
            return default(T?); // null

        Type type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return Storage.GetValue<T>(path);
    }

    protected T? GetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSource>(string key, IDataTypeConverter<TSource, T> converter)
        where T: notnull
        where TSource: notnull
    {
        var path = GetPathFromKey(key);

        if (!Storage.Contains(path))
            return default(T?); // null

        // If a type converter is given, use it to convert the value; otherwise, return the value as the type stored
        TSource value = Storage.GetValue<TSource>(path);
        return (T?)converter.Convert(value);
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue">Cannot be null; the setting item is removed if set to null.</param>
    /// <returns></returns>
    protected T GetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSource>(string key, T defaultValue, IDataTypeConverter<TSource, T> converter)
        where T : notnull
        where TSource : notnull
    {
        if (defaultValue is null)
            throw new ArgumentNullException(nameof(defaultValue));

        var path = GetPathFromKey(key);

        TSource defaultValueConverted = converter.ConvertFrom(defaultValue);

        if (!Storage.Contains(path)) // key is not found
        {
            Storage.SetValue(path, defaultValueConverted);
            return defaultValue;
        }

        // Try to get the value from the storage
        T? value = GetValue(key, converter);
        if (value is null) 
        {
            Storage.SetValue(path, defaultValueConverted);
            return defaultValue;
        }

        return value;
    }

    protected T GetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string key, T defaultValue) where T : notnull
    {
        if (defaultValue is null)
            throw new ArgumentNullException(nameof(defaultValue));

        var path = GetPathFromKey(key);

        if (!Storage.Contains(path)) // key is not found
        {
            Storage.SetValue(path, defaultValue);
            return defaultValue;
        }

        // Try to get the value from the storage
        T? value = GetValue<T>(key);
        if (value is null)
        {
            Storage.SetValue(path, defaultValue);
            return defaultValue;
        }

        return value;
    }

    #endregion

    #region SetValue<T>

    private void SetValueInternal<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSource>(string key,
                              T value,
                              SettingChangedEventHandler? _event,
                              IDataTypeConverter<TSource, T> converter)
        where T : notnull
        where TSource: notnull
    {
        var path = GetPathFromKey(key);
        T? currentValue = Storage.Contains(path) ? converter.Convert(Storage.GetValue<TSource>(path)) : default;

        if (value.Equals(currentValue))
            return;

        // Only set the value when the new value is different
        object? convertedValue = converter.ConvertFrom(value);
        if(convertedValue is null)
            Storage.DeleteItem(path);
        else
            Storage.SetValue(path, convertedValue);

        // Notify a setting item has been changed
        _event?.Invoke(this, new SettingChangedEventArgs(path, value));
        SettingsChanged?.Invoke(this, new SettingChangedEventArgs(path, value));
    }

    private void SetValueInternal<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string key,
                              T value,
                              SettingChangedEventHandler? _event)
        where T : notnull
    {
        var path = GetPathFromKey(key);
        T? currentValue = Storage.Contains(path) ? Storage.GetValue<T>(path) : default;

        if (value.Equals(currentValue))
            return;

        // No converter is given; store the value as type T.
        Storage.SetValue<T>(path, value);

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
    protected void SetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSource>(string key,
                               T? value,
                               SettingChangedEventHandler? _event,
                               IDataTypeConverter<TSource, T> converter)
        where T : struct
        where TSource : notnull
    {
        if (value is null)
        {
            Storage.DeleteItem(key);
            return;
        }

        SetValueInternal<T, TSource>(key, (T)value, _event, converter);
    }

    protected void SetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string key,
                           T? value,
                           SettingChangedEventHandler? _event)
        where T : struct
    {
        if (value is null)
        {
            Storage.DeleteItem(key);
            return;
        }

        SetValueInternal<T>(key, (T)value, _event);
    }

    /// <summary>
    /// Wrap SetValue for reference types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="_event"></param>
    protected void SetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TSource>(string key,
                               T? value,
                               SettingChangedEventHandler? _event,
                               IDataTypeConverter<TSource, T> converter)
        where T : class
        where TSource : notnull
    {
        if (value is null)
        {
            Storage.DeleteItem(key);
            return;
        }
        SetValueInternal<T, TSource>(key, value, _event, converter);
    }

    protected void SetValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string key,
                           T? value,
                           SettingChangedEventHandler? _event)
    where T : class
    {
        if (value is null)
        {
            Storage.DeleteItem(key);
            return;
        }
        SetValueInternal<T>(key, value, _event);
    }

    #endregion SetValue<T>
}