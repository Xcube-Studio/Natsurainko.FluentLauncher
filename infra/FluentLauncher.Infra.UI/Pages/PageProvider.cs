using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.UI.Pages;

public abstract class PageProvider<TPageBase> : IPageProvider
{
    protected readonly IServiceProvider _pageProvider;
    protected readonly IReadOnlyDictionary<string, PageDescriptor> _registeredPages;

    public IReadOnlyDictionary<string, PageDescriptor> RegisteredPages => _registeredPages;

    public PageProvider(IReadOnlyDictionary<string, PageDescriptor> registeredPages, IServiceProvider pageProvider)
    {
        _registeredPages = registeredPages;
        _pageProvider = pageProvider;
    }

    public object GetPage(string key)
    {
        var pageType = _registeredPages[key].PageType;
        var vmType = _registeredPages[key].ViewModelType;

        if (vmType is null)
        {
            return _pageProvider.GetRequiredService(pageType);
        }
        else
        {
            var page = (TPageBase)_pageProvider.GetRequiredService(pageType);
            var vm = _pageProvider.GetRequiredService(vmType);
            ConfigureViewModel(page, vm);
            return page;
        }
    }

    public object? GetViewModel(string key)
    {
        var vmType = _registeredPages[key].ViewModelType;

        if (vmType is null)
            return null;

        return _pageProvider.GetRequiredService(vmType);
    }

    protected abstract void ConfigureViewModel(TPageBase page, object viewModel);
}
