using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Windows;

public class WinUIActivationService : ActivationService<Window>
{
    public static WinUIActivationServiceBuilder GetBuilder(IServiceProvider serviceProvider)
    {
        return new WinUIActivationServiceBuilder(serviceProvider);
    }

    public WinUIActivationService(IReadOnlyDictionary<string, WindowDescriptor> registeredWindows, IServiceProvider serviceProvider)
        : base(registeredWindows, serviceProvider) { }

    protected override IWindowService ActivateWindow(Window window)
    {
        window.Activate();
        var windowService = new WinUIWindowService(window);
        _activeWindows.Add((window, windowService));
        return windowService;
    }

    protected override void ConfigureWindowClose(Window window, IServiceScope scope)
    {
        window.Closed += (_, _) =>
        {
            scope.Dispose();
            for (int i = 0; i < _activeWindows.Count; i++)
            {
                if (_activeWindows[i].Item1 == window)
                {
                    _activeWindows.RemoveAt(i);
                    break;
                }
            }
        };
    }
}
