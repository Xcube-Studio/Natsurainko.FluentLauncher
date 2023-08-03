using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Windows;

/// <summary>
/// Default implementation of <see cref="IActivationService"/>
/// </summary>
abstract class ActivationService<TWindowBase> : IActivationService
{
    protected readonly IServiceProvider _windowProvider;
    protected readonly IReadOnlyDictionary<string, WindowDescriptor> _registeredWindows;

    public IReadOnlyDictionary<string, WindowDescriptor> RegisteredWindows => _registeredWindows;

    internal ActivationService(IReadOnlyDictionary<string, WindowDescriptor> registeredWindows, IServiceProvider windowProvider)
    {
        _registeredWindows = registeredWindows;
        _windowProvider = windowProvider;
    }

    public IWindowService ActivateWindow(string key)
    {
        Type windowType = RegisteredWindows[key].WindowType; // windowType is guaranteed to be a subclass of TWindowBase when the activation service is built
        TWindowBase window = (TWindowBase)_windowProvider.GetService(windowType);
        return ActivateWindow(window);
    }

    protected abstract IWindowService ActivateWindow(TWindowBase window);
}
