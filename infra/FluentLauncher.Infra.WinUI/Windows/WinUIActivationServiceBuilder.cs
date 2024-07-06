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
    public WinUIActivationServiceBuilder(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

    public override IActivationService Build()
    {
        return new WinUIActivationService(_registeredWindows, _serviceProvider);
    }
}
