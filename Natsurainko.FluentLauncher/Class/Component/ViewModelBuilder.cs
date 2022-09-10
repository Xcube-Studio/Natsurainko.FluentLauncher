using Natsurainko.FluentLauncher.ViewModel;
using System;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Class.Component;

public static class ViewModelBuilder
{
    public static ViewModelBase<TControl> Build<TControl>(TControl control) where TControl : Control
    {
        var t = Type.GetType($"{control.GetType().FullName.Replace("View", "ViewModel")}VM");

        if (t != null)
            return (ViewModelBase<TControl>)Activator.CreateInstance(t, new object[] { control });

        return null;
    }

    public static TResult Build<TResult, TControl>(TControl control)
        where TControl : Control
        where TResult : ViewModelBase<TControl>
    {
        var t = Type.GetType($"{control.GetType().FullName.Replace("View", "ViewModel")}VM");

        if (t != null)
            return Activator.CreateInstance(typeof(TResult), control) as TResult;

        return null;
    }
}
