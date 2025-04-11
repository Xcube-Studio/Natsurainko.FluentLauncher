using FluentLauncher.Infra.UI.Pages;
using FluentLauncher.Infra.WinUI.Mvvm;
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
    {
        page.DataContext = viewModel;

        if (viewModel is IViewAssociated viewAssociatedModel)
        {
            viewAssociatedModel.Dispatcher = page.DispatcherQueue;

            page.Loading += (_, _) => viewAssociatedModel.OnLoading();
            page.Loaded += (_, _) => viewAssociatedModel.OnLoaded();
            page.Unloaded += (_, _) => viewAssociatedModel.OnUnloaded();

            if (viewAssociatedModel is IViewAssociated<Page> pageAssociatedModel)
                pageAssociatedModel.SetView(page);
        }
    }
}
