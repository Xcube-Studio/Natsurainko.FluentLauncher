using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FluentLauncher.Infra.UI.Windows;

/// <summary>
/// Builder of any type derived from <see cref="ActivationService{TWindowBase}"/>
/// </summary>
/// <typeparam name="TService">Type of the activation service</typeparam>
/// <typeparam name="TWindowBase">Base type of the window managed by the activation service</typeparam>
public class ActivationServiceBuilder<TService, TWindowBase> where TService : ActivationService<TWindowBase>
{
    private readonly Dictionary<string, WindowDescriptor> _registeredWindows = new();
    private readonly IServiceProvider _windowProvider;
    private Func<IReadOnlyDictionary<string, WindowDescriptor>, IServiceProvider, TService> _serviceFactory;

    public ActivationServiceBuilder(IServiceProvider windowProvider)
    {
        _windowProvider = windowProvider;
    }

    public ActivationServiceBuilder<TService, TWindowBase> WithServiceFactory(
        Func<IReadOnlyDictionary<string, WindowDescriptor>, IServiceProvider, TService> serviceFactory)
    {
        _serviceFactory = serviceFactory;
        return this;
    }

    /// <summary>
    /// Add a window type to the list of registered windows.
    /// </summary>
    /// <param name="key">Key of the window</param>
    /// <param name="windowType">Type of the window</param>
    /// <param name="multiInstance">Whether multiple instances of the window is allowed</param>
    public ActivationServiceBuilder<TService, TWindowBase> WithWindow(string key, Type windowType, bool multiInstance)
    {
        if (!windowType.IsSubclassOf(typeof(TWindowBase)))
            throw new ArgumentException($"Type {windowType} is not a subclass of {typeof(TWindowBase)}");

        _registeredWindows.Add(key, new(windowType, multiInstance));
        return this;
    }

    public ActivationServiceBuilder<TService, TWindowBase> WithSingleInstanceWindow(string key, Type windowType) => WithWindow(key, windowType, false);
    public ActivationServiceBuilder<TService, TWindowBase> WithSingleInstanceWindow<TWindow>(string key) => WithSingleInstanceWindow(key, typeof(TWindow));
    public ActivationServiceBuilder<TService, TWindowBase> WithMultiInstanceWindow(string key, Type windowType) => WithWindow(key, windowType, true);
    public ActivationServiceBuilder<TService, TWindowBase> WithMultiInstanceWindow<TWindow>(string key) => WithMultiInstanceWindow(key, typeof(TWindow));

    public TService Build()
    {
        var registeredWindows = new ReadOnlyDictionary<string, WindowDescriptor>(_registeredWindows);
        return _serviceFactory(registeredWindows, _windowProvider);
    }
}
