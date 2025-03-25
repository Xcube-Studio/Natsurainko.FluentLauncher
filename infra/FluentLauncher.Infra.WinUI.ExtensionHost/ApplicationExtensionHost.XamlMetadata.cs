using FluentLauncher.Infra.WinUI.ExtensionHost.Utils;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentLauncher.Infra.WinUI.ExtensionHost;

public partial class ApplicationExtensionHost<TApplication>
{
    private static readonly PropertyInfo MetadataProviderProperty =
        typeof(TApplication).GetProperty("_AppProvider", BindingFlags.NonPublic | BindingFlags.Instance, null, null, [], null) 
            ?? throw new AccessViolationException();

    private static readonly PropertyInfo TypeInfoProviderProperty =
        MetadataProviderProperty.PropertyType.GetProperty("Provider", BindingFlags.NonPublic | BindingFlags.Instance, null, null, [], null) 
            ?? throw new AccessViolationException();

    private static readonly PropertyInfo OtherProvidersProperty =
        TypeInfoProviderProperty.PropertyType.GetProperty("OtherProviders", BindingFlags.NonPublic | BindingFlags.Instance, null, typeof(List<IXamlMetadataProvider>), [], null)
            ?? throw new AccessViolationException();

    private List<IXamlMetadataProvider> OtherProviders
    {
        get
        {
            object appProvider = MetadataProviderProperty.GetValue(Application) ?? throw new AccessViolationException();
            object provider = TypeInfoProviderProperty.GetValue(appProvider) ?? throw new AccessViolationException();
            List<IXamlMetadataProvider> otherProviders = (OtherProvidersProperty.GetValue(provider) as List<IXamlMetadataProvider>) ?? throw new AccessViolationException();
            return otherProviders;
        }
    }

    public IDisposable RegisterXamlTypeMetadataProvider(IXamlMetadataProvider provider)
    {
        OtherProviders.Add(provider);
        return new RunOnDispose(() => OtherProviders.Remove(provider));
    }
}
