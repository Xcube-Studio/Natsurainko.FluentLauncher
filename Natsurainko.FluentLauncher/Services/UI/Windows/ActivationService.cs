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
abstract class ActivationService<TWindow> : IActivationService
{
    protected readonly IServiceProvider _windowProvider;
    protected readonly IReadOnlyDictionary<string, (Type windowType, bool multiInstance)> _registeredWindows;

    public IReadOnlyDictionary<string, (Type windowType, bool multiInstance)> RegisteredWindows => _registeredWindows;

    internal ActivationService(IReadOnlyDictionary<string, (Type windowType, bool multiInstance)> registeredWindows)
    {
        _registeredWindows = registeredWindows;

        ServiceCollection windowsCollection = new();
        foreach ((Type type, bool multiInstance) in registeredWindows.Values)
        {
            // No need to check window type. This is an internal constructor that can only be called by the builder.
            //if (type.IsSubclassOf(typeof(TWindow)) == false)
            //    throw new ArgumentException($"Type {type} is not a subclass of {typeof(TWindow)}");

            if (multiInstance)
                windowsCollection.AddTransient(type);
            else
                windowsCollection.AddSingleton(type);
        }
        _windowProvider = windowsCollection.BuildServiceProvider();
    }

    public IWindowService ActivateWindow(string key)
    {
        (Type windowType, bool _) = RegisteredWindows[key];
        TWindow window = (TWindow)_windowProvider.GetService(windowType);
        return ActivateWindow(window);
    }

    protected abstract IWindowService ActivateWindow(TWindow window);

    public void Register(string key, Type windowType, bool multiInstance)
    {
        throw new NotImplementedException();
    }
}
