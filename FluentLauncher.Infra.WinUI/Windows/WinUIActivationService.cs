using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Services.UI.Windowing;

public class WinUIActivationService : ActivationService<Window>
{
    // Factory pattern
    public static ActivationServiceBuilder<WinUIActivationService, Window> GetBuilder(IServiceProvider windowProvider)
    {
        return new ActivationServiceBuilder<WinUIActivationService, Window>(windowProvider)
            .WithServiceFactory((r, p) => new WinUIActivationService(r, p));
    }

    private WinUIActivationService(IReadOnlyDictionary<string, WindowDescriptor> registeredWindows, IServiceProvider windowProvier)
        : base(registeredWindows, windowProvier) { }

    protected override IWindowService ActivateWindow(Window window)
    {
        window.Activate();
        return new WindowService(window);
    }

    protected override void ConfigureWindowClose(Window window, IServiceScope scope)
    {
        window.Closed += (_, _) => scope.Dispose();
    }
}
