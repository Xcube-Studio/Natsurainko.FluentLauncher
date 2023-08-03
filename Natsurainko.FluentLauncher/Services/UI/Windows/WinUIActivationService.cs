using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Windows;

internal class WinUIActivationService : ActivationService<Window>
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
}
