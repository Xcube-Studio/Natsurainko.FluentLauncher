using FluentLauncher.Infra.UI.Pages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Pages;

public class WinUIPageProvider : PageProvider<Page>
{
    public static WinUIPageProviderBuilder CreateBuilder() => new WinUIPageProviderBuilder();

    public WinUIPageProvider(
        IReadOnlyDictionary<string, PageDescriptor> registeredPages,
        IServiceProvider serviceProvider)
        : base(registeredPages, serviceProvider) { }

    protected override void ConfigureViewModel(Page page, object viewModel)
        => page.DataContext = viewModel;
}
