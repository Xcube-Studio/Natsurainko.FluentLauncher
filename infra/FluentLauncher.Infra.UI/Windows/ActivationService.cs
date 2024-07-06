using FluentLauncher.Infra.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.UI.Windows;

/// <summary>
/// Default implementation of <see cref="IActivationService"/>
/// </summary>
public abstract class ActivationService<TWindowBase> : IActivationService
{
    protected readonly IServiceProvider _windowProvider;
    protected readonly IReadOnlyDictionary<string, WindowDescriptor> _registeredWindows;
    protected readonly List<TWindowBase> _activeWindows = new(); // Tracks all active windows for checking windows registered as SingleInstance

    public IReadOnlyDictionary<string, WindowDescriptor> RegisteredWindows => _registeredWindows;

    /// <summary>
    /// Build an activation service that supports activating the windows described
    /// </summary>
    /// <param name="registeredWindows">A read only dictionary that maps string keys to <see cref="WindowDescriptor"/> objects.</param>
    /// <param name="windowProvider">An <see cref="IServiceProvider"/> that has been configured to support window types according to the rules defined by <paramref name="registeredWindows"/>.</param>
    public ActivationService(IReadOnlyDictionary<string, WindowDescriptor> registeredWindows, IServiceProvider windowProvider)
    {
        _registeredWindows = registeredWindows;
        _windowProvider = windowProvider;
    }

    public IWindowService ActivateWindow(string key)
    {
        // Creates a new scope for resources owned by the window
        IServiceScope scope = _windowProvider.CreateScope();

        // Constructs the window
        Type windowType = RegisteredWindows[key].WindowType; // windowType is guaranteed to be a subclass of TWindowBase when the activation service is built
        TWindowBase window = (TWindowBase)scope.ServiceProvider.GetRequiredService(windowType);

        // If the window supports navigation, initialize the navigation service for the window scope
        // The navigation service may have been instantiated and injected into 'window' already.
        if (window is INavigationProvider navProvider)
        {
            var navService = scope.ServiceProvider.GetRequiredService<INavigationService>();
            navService.InitializeNavigation(navProvider, scope, null);
        }

        // Configures the scope to be disposed when the window is closed
        ConfigureWindowClose(window, scope);

        // Activates the window
        return ActivateWindow(window);
    }

    /// <summary>
    /// Activate the <paramref name="window"/> resolved and return an <see cref="IWindowService"/> that can be used to control it.
    /// </summary>
    /// <remarks>Must update <see cref="_activeWindows"/> after the <paramref name="window"/> is successfully activated.</remarks>
    /// <param name="window"></param>
    /// <returns></returns>
    protected abstract IWindowService ActivateWindow(TWindowBase window);

    /// <summary>
    /// Configure the <paramref name="window"/> to dispose the <paramref name="scope"/> and removes itself from ActiveWindows when it is closed.
    /// </summary>
    /// <remarks>Must update <see cref="_activeWindows"/> after the <paramref name="window"/> is closed.</remarks>
    /// <param name="window"></param>
    /// <param name="scope"></param>
    protected abstract void ConfigureWindowClose(TWindowBase window, IServiceScope scope);
}
