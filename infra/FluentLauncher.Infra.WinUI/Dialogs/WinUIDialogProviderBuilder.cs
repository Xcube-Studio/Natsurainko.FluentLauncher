using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Pages;
using FluentLauncher.Infra.UI.Windows;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.WinUI.Dialogs;

public class WinUIDialogProviderBuilder : DialogProviderBuilder<WinUIDialogProvider, ContentDialog>
{
    public new Dictionary<string, DialogDescriptor> RegisteredDialogs => _registeredDialogs;

    public override WinUIDialogProvider Build(IServiceProvider serviceProvider)
    {
        return new WinUIDialogProvider(_registeredDialogs, serviceProvider);
    }
}
