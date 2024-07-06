using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Windows;

public class WinUIActivationService : ActivationService<Window>
{
    // Factory pattern
    public static ActivationServiceBuilder<WinUIActivationService, Window> GetBuilder(IServiceProvider serviceProvider)
    {
        return new ActivationServiceBuilder<WinUIActivationService, Window>(serviceProvider)
            .WithServiceFactory((r, p) => new WinUIActivationService(r, p));
    }

    private WinUIActivationService(IReadOnlyDictionary<string, WindowDescriptor> registeredWindows, IServiceProvider serviceProvider)
        : base(registeredWindows, serviceProvider) { }

    protected override IWindowService ActivateWindow(Window window)
    {
        window.Activate();
        var windowService = new WinUIWindowService(window);
        _activeWindows.Add(window);
        return windowService;
    }

    protected override void ConfigureWindowClose(Window window, IServiceScope scope)
    {
        window.Closed += (_, _) =>
        {
            scope.Dispose();
            _activeWindows.Remove(window);
        };
    }
}
