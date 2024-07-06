using FluentLauncher.Infra.UI.Pages;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Pages;

public class WinUIPageProvider : PageProvider<Page>
{
    // Factory pattern
    public static PageProviderBuilder<WinUIPageProvider, Page> GetBuilder(IServiceProvider serviceProvider)
    {
        return new PageProviderBuilder<WinUIPageProvider, Page>(serviceProvider)
            .WithPageProviderFactory((r, p) => new WinUIPageProvider(r, p));
    }

    public WinUIPageProvider(IReadOnlyDictionary<string, PageDescriptor> registeredPages, IServiceProvider serviceProvider)
        : base(registeredPages, serviceProvider) { }

    protected override void ConfigureViewModel(Page page, object viewModel)
        => page.DataContext = viewModel;
}
