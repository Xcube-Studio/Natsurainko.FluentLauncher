using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI.Windows;

public record WindowDescriptor
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type WindowType { get; init; }

    public bool AllowMultiInstances { get; init; }

    public WindowDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type windowType,
        bool allowMultiInstances = false)
    {
        WindowType = windowType;
        AllowMultiInstances = allowMultiInstances;
    }
}

/// <summary>
/// A service for activating app windows.
/// A window can be registered as single instance or multiple instances with a string key.
/// </summary>
public interface IActivationService
{
    /// <summary>
    /// A dictionary of registered windows that maps a string key to a window type.
    /// </summary>
    IReadOnlyDictionary<string, WindowDescriptor> RegisteredWindows { get; }

    /// <summary>
    /// Attempts to activate a registered window
    /// </summary>
    /// <remarks>
    /// If the window is registered as single instance, it will activate the existing instance
    /// by bringing it to the foreground, or create a new instance if there is no instance of this type.
    /// <br/>
    /// If the window is registered as multiple instances, it will create a new instance of the window,
    /// and activate the window by bringing it to the foreground.
    /// <br/>
    /// If the window is not registered, it will throw an exception.
    /// </remarks>
    /// <param name="key">Key of the window</param>
    /// <returns>IWindowService of the window created and/or activated</returns>
    IWindowService ActivateWindow(string key); // TODO: allow passing parameters to the window
}
