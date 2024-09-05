using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentLauncher.Infra.Settings.Converters;

namespace FluentLauncher.Infra.Settings;

public class SettingsCollection<T> : ObservableCollection<T> where T : notnull
{
    private readonly ISettingsStorage _settingsStorage;
    private readonly string _storagePath;

    public SettingsCollection(ISettingsStorage settingsStorage, string storagePath)
    {
        _settingsStorage = settingsStorage;
        _storagePath = storagePath;

        // Init the storage with an empty array if it doesn't exist
        if (!_settingsStorage.Contains(storagePath))
        {
            _settingsStorage.SetValue(storagePath, Array.Empty<T>());
        }
        
        // Load initial values from storage
        T[] storedArray = _settingsStorage.GetValue<T[]>(storagePath);

        foreach (T value in storedArray)
            Add(value);
    
        // Register CollectionChanged event after initialization
        CollectionChanged += SettingsCollection_CollectionChanged;
    }

    private void SettingsCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Save the entire list as an array in the storage
        T[] array = this.ToArray();
        _settingsStorage.SetValue<T[]>(_storagePath, array);
    }
}

public class SettingsCollection<T,TSource> : ObservableCollection<T> where T : notnull
{
    private readonly ISettingsStorage _settingsStorage;
    private readonly IDataTypeConverter<TSource, T> _typeConverter;
    private readonly string _storagePath;

    public SettingsCollection(ISettingsStorage settingsStorage, string storagePath, IDataTypeConverter<TSource, T> typeConverter)
    {
        _settingsStorage = settingsStorage;
        _storagePath = storagePath;
        _typeConverter = typeConverter;

        // Init the storage with an empty array if it doesn't exist
        if (!_settingsStorage.Contains(storagePath))
        {
            _settingsStorage.SetValue(storagePath, Array.Empty<TSource>());
        }

        // Load initial values from storage

        // Get the array from the storage and apply type conversion
        if (typeConverter is not null)
        {
            TSource[] storedArray = _settingsStorage.GetValue<TSource[]>(storagePath);

            // Convert each element of the array and add it to the collection
            IEnumerator enumerator = storedArray.GetEnumerator();
            while (enumerator.MoveNext())
            {
                T convertedValue = typeConverter.Convert((TSource)enumerator.Current);
                Add(convertedValue);
            }
        }
        else // No type conversion needed
        {
            T[] storedArray = _settingsStorage.GetValue<T[]>(storagePath);

            foreach (T value in storedArray)
                Add(value);
        }

        // Register CollectionChanged event after initialization
        CollectionChanged += SettingsCollection_CollectionChanged;
    }

    private void SettingsCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        TSource[] convertedArray = new TSource[Count];
        this.Select(item => _typeConverter.ConvertFrom(item)).ToArray().CopyTo(convertedArray, 0);
        _settingsStorage.SetValue(_storagePath, convertedArray);
    }
}
