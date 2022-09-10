using ReactiveUI;
using Splat;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel;

public class ViewModelBase<TControl> : ReactiveObject where TControl : Control
{
    public TControl Control { get; private set; }

    public ViewModelBase(TControl control)
    {
        Control = control;

        Control.Loaded += Control_Loaded;
        Control.Unloaded += Control_Unloaded;
    }

    private void Control_Loaded(object sender, RoutedEventArgs e)
    {
        this.PropertyChanged += ViewModelBase_PropertyChanged;

        Control.DataContext = this;
        Control.Loaded -= Control_Loaded;
    }

    private void Control_Unloaded(object sender, RoutedEventArgs e)
    {
        this.PropertyChanged -= ViewModelBase_PropertyChanged;
        Control.Unloaded -= Control_Unloaded;
    }

    protected void LogException(Exception exception)
    {
        Debug.WriteLine(exception.StackTrace);
        this.Log().Error(exception);
    }

    private void ViewModelBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        => OnPropertyChanged(e);

    public virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {

    }
}
