using FluentLauncher.Infra.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.UI.Windows;

/// <summary>
/// Default implementation of <see cref="IActivationService"/>
/// </summary>
public abstract class ActivationService<TWindowBase> : IActivationService where TWindowBase : notnull
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IReadOnlyDictionary<string, WindowDescriptor> _registeredWindows;
    protected readonly List<(TWindowBase, IWindowService)> _activeWindows = new(); // Tracks all active windows for checking windows registered as SingleInstance

    public IReadOnlyDictionary<string, WindowDescriptor> RegisteredWindows => _registeredWindows;

    /// <summary>
    /// Build an activation service that supports activating the windows described
    /// </summary>
    /// <param name="registeredWindows">A read only dictionary that maps string keys to <see cref="WindowDescriptor"/> objects.</param>
    /// <param name="serviceProvider">An <see cref="IServiceProvider"/> that has been configured to support window types according to the rules defined by <paramref name="registeredWindows"/>.</param>
    public ActivationService(IReadOnlyDictionary<string, WindowDescriptor> registeredWindows, IServiceProvider serviceProvider)
    {
        _registeredWindows = registeredWindows;
        _serviceProvider = serviceProvider;
    }

    public IWindowService ActivateWindow(string key)
    {
        var windowDescriptor = RegisteredWindows[key];
        Type windowType = windowDescriptor.WindowType; // windowType is guaranteed to be a subclass of TWindowBase when the activation service is built

        // Check for single-instance windows
        if (!windowDescriptor.AllowMultiInstances)
        {
            foreach (var (w, s) in _activeWindows)
            {
                if (w.GetType() == windowType)
                {
                    // The single-instance window exists already
                    s.Activate();
                    return s;
                }
            }
        }

        // Creates a root scope for resources owned by the window
        IServiceScope scope = _serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IServiceScopeHierarchy>().Initialize(null, scope);

        TWindowBase window = (TWindowBase)scope.ServiceProvider.GetRequiredService(windowType);
        IWindowService windowService = scope.ServiceProvider.GetRequiredService<IWindowService>();
        InitializeWindowService(windowService, window);
        _activeWindows.Add((window, windowService));

        // If the window supports navigation, initialize the navigation service in the window scope
        // The navigation service may have been instantiated and injected into 'window' already.
        if (window is INavigationProvider navigationProvider)
        {
            INavigationService navigationService = scope.ServiceProvider.GetRequiredService<INavigationService>();
            InitializeNavigationService(navigationService, navigationProvider);
        }

        // Configures the scope to be disposed when the window is closed
        ConfigureWindowClose(window, scope);

        // Activates the window
        ActivateWindow(window);

        return windowService;
    }

    /// <summary>
    /// Initialize the <paramref name="windowService"/> for the given <paramref name="window"/>.
    /// </summary>
    /// <param name="windowService"></param>
    /// <param name="window"></param>
    protected abstract void InitializeWindowService(IWindowService windowService, TWindowBase window);

    /// <summary>
    /// Initialize the <paramref name="navigationService"/> for the given <paramref name="navigationProvider"/>.
    /// </summary>
    /// <param name="navigationService"></param>
    /// <param name="navigationProvider"></param>
    protected abstract void InitializeNavigationService(INavigationService navigationService, INavigationProvider navigationProvider);

    /// <summary>
    /// Activate the <paramref name="window"/> resolved and return an <see cref="IWindowService"/> that can be used to control it.
    /// </summary>
    /// <remarks>Must update <see cref="_activeWindows"/> after the <paramref name="window"/> is successfully activated.</remarks>
    /// <param name="window"></param>
    /// <returns></returns>
    protected abstract void ActivateWindow(TWindowBase window);

    /// <summary>
    /// Configure the <paramref name="window"/> to dispose the <paramref name="scope"/> and removes itself from ActiveWindows when it is closed.
    /// </summary>
    /// <remarks>Must update <see cref="_activeWindows"/> after the <paramref name="window"/> is closed.</remarks>
    /// <param name="window"></param>
    /// <param name="scope"></param>
    protected abstract void ConfigureWindowClose(TWindowBase window, IServiceScope scope);
}
