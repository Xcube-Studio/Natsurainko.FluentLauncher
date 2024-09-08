using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI.Pages;

public abstract class PageProviderBuilder<TPageProvider, TPageBase> where TPageProvider : PageProvider<TPageBase>
{
    protected readonly Dictionary<string, PageDescriptor> _registeredPages = new();

    public IDictionary<string, PageDescriptor> RegisteredPages => _registeredPages;

    public PageProviderBuilder() { }

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPage, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TViewModel>(string key)
        => WithPage(key, typeof(TPage), typeof(TViewModel));

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPage>(string key)
        => WithPage(key, typeof(TPage));

    public PageProviderBuilder<TPageProvider, TPageBase> WithPage(
        string key,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type pageType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type? viewModelType = null)
    {
        _registeredPages.Add(key, new PageDescriptor(pageType, viewModelType));
        return this;
    }

    public abstract TPageProvider Build(IServiceProvider serviceProvider);
}
