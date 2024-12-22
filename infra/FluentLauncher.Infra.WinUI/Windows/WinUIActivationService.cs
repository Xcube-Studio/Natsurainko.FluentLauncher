using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Windows;

public class WinUIActivationService : ActivationService<Window>
{
    public static WinUIActivationServiceBuilder CreateBuilder() => new WinUIActivationServiceBuilder();

    public WinUIActivationService(
        IReadOnlyDictionary<string, WindowDescriptor> registeredWindows,
        IServiceProvider serviceProvider)
        : base(registeredWindows, serviceProvider) { }

    protected override void InitializeWindowService(IWindowService windowService, Window window)
    {
        ((WinUIWindowService)windowService).InitializeService(window);
    }

    protected override void ActivateWindow(Window window)
    {
        window.Activate();
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
