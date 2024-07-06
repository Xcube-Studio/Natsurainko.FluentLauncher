using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.UI.Pages;

public class PageProviderBuilder<TPageProvider, TPageBase> where TPageProvider : PageProvider<TPageBase>
{
    private readonly Dictionary<string, PageDescriptor> _registeredPages = new();
    private readonly IServiceProvider _serviceProvider;

    private Func<IReadOnlyDictionary<string, PageDescriptor>, IServiceProvider, TPageProvider>? _pageProviderFactory;

    public PageProviderBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public PageProviderBuilder<TPageProvider, TPageBase> WithPageProviderFactory(
        Func<IReadOnlyDictionary<string, PageDescriptor>, IServiceProvider, TPageProvider> factory)
    {
        _pageProviderFactory = factory;
        return this;
    }

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage<TPage, TViewModel>(string key)
        => WithPage(key, typeof(TPage), typeof(TViewModel));

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage<TPage>(string key)
        => WithPage(key, typeof(TPage));

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage(string key, Type pageType, Type viewModelType = null)
    {
        _registeredPages.Add(key, new PageDescriptor(pageType, viewModelType));
        return this;
    }

    public TPageProvider Build()
    {
        if (_pageProviderFactory is null)
            throw new InvalidOperationException("IServiceProvider factory is required");

        return _pageProviderFactory(_registeredPages, _serviceProvider);
    }
}
