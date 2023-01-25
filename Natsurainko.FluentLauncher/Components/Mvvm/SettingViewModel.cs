using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Natsurainko.FluentLauncher.Components.Mvvm;

public partial class SettingViewModel : ObservableObject
{
    protected bool loading = true;

    public SettingViewModel()
    {
        foreach (var property in GetType().GetProperties())
        {
            var configProperty = typeof(Configuration).GetProperty(property.Name);

            if (configProperty == null)
                continue;

            if (property.GetValue(this) == configProperty.GetValue(App.Configuration))
                OnPropertyChanged(property.Name);

            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
            {
                var instance = Activator.CreateInstance(property.PropertyType,
                    args: new object[] { configProperty.GetValue(App.Configuration) });
                property.SetValue(this, instance);
            }
            else property.SetValue(this, configProperty.GetValue(App.Configuration));
        }

        loading = false;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        _OnPropertyChanged(e);

        if (loading)
            return;

        var property = GetType().GetProperty(e.PropertyName);
        var configProperty = typeof(Configuration).GetProperty(property.Name);

        if (configProperty == null)
            return;

        if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
        {
            var instance = typeof(Enumerable)
                .GetMethod("ToList")
                .MakeGenericMethod(property.PropertyType.GetGenericArguments())
                .Invoke(null, new object[] { property.GetValue(this) });
            configProperty.SetValue(App.Configuration, instance);

            return;
        }

        configProperty.SetValue(App.Configuration, property.GetValue(this));
    }

    protected virtual void _OnPropertyChanged(PropertyChangedEventArgs e) { }
}
