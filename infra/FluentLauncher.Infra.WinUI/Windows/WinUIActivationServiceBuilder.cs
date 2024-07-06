using FluentLauncher.Infra.UI.Windows;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.WinUI.Windows;

public class WinUIActivationServiceBuilder : ActivationServiceBuilder<WinUIActivationService, Window>
{
    public override WinUIActivationService Build(IServiceProvider serviceProvider)
    {
        return new WinUIActivationService(_registeredWindows, serviceProvider);
    }
}
