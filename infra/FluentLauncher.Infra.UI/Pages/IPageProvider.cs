using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.UI.Pages;

public interface IPageProvider
{
    IReadOnlyDictionary<string, PageDescriptor> RegisteredPages { get; }

    object GetPage(string key);

    object? GetViewModel(string key);
}

public record PageDescriptor
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type PageType { get; init; }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? ViewModelType { get; init; }

    public PageDescriptor(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type pageType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type? vmType)
    {
        PageType = pageType;
        ViewModelType = vmType;
    }
}
