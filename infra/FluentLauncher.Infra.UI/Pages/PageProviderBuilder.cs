using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.UI.Pages;

public abstract class PageProviderBuilder<TPageProvider, TPageBase> where TPageProvider : PageProvider<TPageBase>
{
    protected readonly Dictionary<string, PageDescriptor> _registeredPages = new();

    public IDictionary<string, PageDescriptor> RegisteredPages => _registeredPages;

    public PageProviderBuilder() { }

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage<TPage, TViewModel>(string key)
        => WithPage(key, typeof(TPage), typeof(TViewModel));

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage<TPage>(string key)
        => WithPage(key, typeof(TPage));

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage(string key, Type pageType, Type? viewModelType = null)
    {
        _registeredPages.Add(key, new PageDescriptor(pageType, viewModelType));
        return this;
    }

    public abstract TPageProvider Build(IServiceProvider serviceProvider);
}
