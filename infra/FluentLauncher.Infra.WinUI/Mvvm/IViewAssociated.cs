using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace FluentLauncher.Infra.WinUI.Mvvm;

public interface IViewAssociated
{
    DispatcherQueue Dispatcher { get; set; }

    void OnLoaded();

    void OnUnloaded();
}

public interface IViewAssociated<IView> : IViewAssociated 
    where IView : Control
{
    IView View { get; set; }
}