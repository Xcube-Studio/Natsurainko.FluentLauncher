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
    public static ActivationServiceBuilder<WinUIActivationService, Window> GetBuilder()
    {
        return new ActivationServiceBuilder<WinUIActivationService, Window>().WithServiceFactory(e => new WinUIActivationService(e));
    }

    private WinUIActivationService(IReadOnlyDictionary<string, (Type windowType, bool multiInstance)> registeredWindows) 
        : base(registeredWindows) { }

    public override IWindowService ActivateWindow(Window window)
    {
        window.Activate();
        throw new NotImplementedException();
    }
}
