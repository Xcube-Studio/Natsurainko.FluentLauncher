using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI.Windows;

/// <summary>
/// Builder of any type derived from <see cref="ActivationService{TWindowBase}"/>
/// </summary>
/// <typeparam name="TService">Type of the activation service</typeparam>
/// <typeparam name="TWindowBase">Base type of the window managed by the activation service</typeparam>
public abstract class ActivationServiceBuilder<TService, TWindowBase>
    where TService : ActivationService<TWindowBase>
    where TWindowBase : notnull
{
    protected readonly Dictionary<string, WindowDescriptor> _registeredWindows = new();

    public IDictionary<string, WindowDescriptor> RegisteredWindows => _registeredWindows;

    public ActivationServiceBuilder() { }

    /// <summary>
    /// Add a window type to the list of registered windows.
    /// </summary>
    /// <param name="key">Key of the window</param>
    /// <param name="windowType">Type of the window</param>
    /// <param name="multiInstance">Whether multiple instances of the window is allowed</param>
    public ActivationServiceBuilder<TService, TWindowBase> WithWindow(string key, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type windowType, bool multiInstance)
    {
        if (!windowType.IsSubclassOf(typeof(TWindowBase)))
            throw new ArgumentException($"Type {windowType} is not a subclass of {typeof(TWindowBase)}");

        _registeredWindows.Add(key, new(windowType, multiInstance));
        return this;
    }

    public ActivationServiceBuilder<TService, TWindowBase> AddSingleInstanceWindow(string key, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type windowType)
        => WithWindow(key, windowType, false);

    public ActivationServiceBuilder<TService, TWindowBase> AddSingleInstanceWindow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWindow>(string key)
        => AddSingleInstanceWindow(key, typeof(TWindow));

    public ActivationServiceBuilder<TService, TWindowBase> AddMultiInstanceWindow(string key, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type windowType)
        => WithWindow(key, windowType, true);

    public ActivationServiceBuilder<TService, TWindowBase> AddMultiInstanceWindow<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TWindow>(string key)
        => AddMultiInstanceWindow(key, typeof(TWindow));

    public abstract IActivationService Build(IServiceProvider serviceProvider);
}
