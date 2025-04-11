using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace FluentLauncher.Infra.WinUI.Mvvm;

public interface IViewAssociated
{
    DispatcherQueue Dispatcher { get; set; }

    void OnLoading();

    void OnLoaded();

    void OnUnloaded();
}

public interface IViewAssociated<out TView> : IViewAssociated
    where TView : Control
{
    TView View { get; }

    void SetView(object view);
}