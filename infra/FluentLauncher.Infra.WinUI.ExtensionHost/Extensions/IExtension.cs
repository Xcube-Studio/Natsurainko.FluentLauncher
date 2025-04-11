using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.ExtensionHost.Extensions;

public interface IExtension
{
    string Name { get; }

    Dictionary<string, (Type, Type)> RegisteredPages { get; }

    Dictionary<string, (Type, Type)> RegisteredDialogs { get; }

    void ConfigureServices(IServiceCollection services);

    void SetServiceProvider(IServiceProvider serviceProvider);

    void SetExtensionFolder(string folder);
}

public interface INavigationProviderExtension
{
    NavigationViewItem[] ProvideNavigationItems();
}