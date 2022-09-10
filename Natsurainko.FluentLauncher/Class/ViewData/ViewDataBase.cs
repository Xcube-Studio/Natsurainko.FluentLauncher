using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class ViewDataBase : ReactiveObject
{
    public ViewDataBase()
    {
        this.PropertyChanged += ViewDataBase_PropertyChanged;
    }

    private void ViewDataBase_PropertyChanged(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(e);

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) { }

    ~ViewDataBase()
    {
        this.PropertyChanged -= ViewDataBase_PropertyChanged;
    }
}

public class ViewDataBase<T> : ViewDataBase
{
    [Reactive]
    public T Data { get; set; }

    public ViewDataBase(T data) : base()
    {
        this.Data = data;
    }
}

public static class ViewDataBaseExtension
{
    public static ViewDataBase<TData> CreateViewData<TData>(this TData data)
        => new(data);

    public static TResult CreateViewData<TData, TResult>(this TData data)
        where TResult : ViewDataBase<TData>
        => Activator.CreateInstance(typeof(TResult), data) as TResult;

    public static ObservableCollection<ViewDataBase<TData>> CreateCollectionViewData<TData>(this IEnumerable<TData> data)
        => new(data.Select(x => new ViewDataBase<TData>(x)));

    public static ObservableCollection<TResult> CreateCollectionViewData<TData, TResult>(this IEnumerable<TData> data)
        where TResult : ViewDataBase<TData>
        => new(data.Select(x => Activator.CreateInstance(typeof(TResult), x) as TResult));
}
