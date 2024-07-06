﻿using FluentLauncher.Infra.UI.Pages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.WinUI.Pages;

public class WinUIPageProviderBuilder : PageProviderBuilder<WinUIPageProvider, Page>
{
    public override WinUIPageProvider Build(IServiceProvider serviceProvider)
    {
        return new WinUIPageProvider(_registeredPages, serviceProvider);
    }
}
