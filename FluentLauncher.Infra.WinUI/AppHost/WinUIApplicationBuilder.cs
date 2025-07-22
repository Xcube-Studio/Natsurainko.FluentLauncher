using FluentLauncher.Infra.UI;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Pages;
using FluentLauncher.Infra.UI.Windows;
using FluentLauncher.Infra.WinUI.Dialogs;
using FluentLauncher.Infra.WinUI.Navigation;
using FluentLauncher.Infra.WinUI.Pages;
using FluentLauncher.Infra.WinUI.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FluentLauncher.Infra.WinUI.AppHost;

public class WinUIApplicationBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApplication> 
    : IHostApplicationBuilder where TApplication : Application
{
    private readonly HostApplicationBuilder _hostApplicationBuilder;

    private bool _useExtendedWinUIServices = false;

    public WinUIActivationServiceBuilder Windows { get; } = new();

    public WinUIPageProviderBuilder Pages { get; } = new();

    public WinUIDialogProviderBuilder Dialogs { get; } = new();

    public WinUIApplicationBuilder()
    {
        _hostApplicationBuilder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            // TODO: pass parameters from ctor
        });

        Services.AddSingleton<TApplication>();
    }

    public WinUIApplication<TApplication> Build()
    {
        if (_useExtendedWinUIServices)
            ConfigureExtendedWinUIServices();

        IHost host = _hostApplicationBuilder.Build();
        return new WinUIApplication<TApplication>(host);
    }

    private void ConfigureExtendedWinUIServices()
    {
        // Configure IParentScopeProvider
        Services.AddScoped<IServiceScopeHierarchy, ServiceScopeHierarchy>();

        // Configure IActivationService
        foreach (var (key, descriptor) in Windows.RegisteredWindows)
        {
            // Always add as scoped for NavigationService; single-instance property is dealt by ActivationService
            Services.AddScoped(descriptor.WindowType);
        }
        Services.AddSingleton<IActivationService, WinUIActivationService>(Windows.Build);

        // Configure IWindowService
        Services.AddScoped<IWindowService>(sp =>
        {
            IServiceScope? parentScope = sp.GetRequiredService<IServiceScopeHierarchy>().ParentScope;
            if (parentScope is null)
                return new WinUIWindowService();
            else
                return parentScope.GetRootScope().ServiceProvider.GetRequiredService<IWindowService>();
        });

        // Configure IDialogProvider
        foreach (var (key, descriptor) in Dialogs.RegisteredDialogs)
        {
            Services.AddTransient(descriptor.DialogType);
            if (descriptor.ViewModelType is not null)
                Services.AddTransient(descriptor.ViewModelType);
        }
        Services.AddSingleton<IDialogProvider, WinUIDialogProvider>(Dialogs.Build);

        // Configure IDialogActivationService
        Services.AddScoped<IDialogActivationService<ContentDialogResult>>(sp =>
        {
            IServiceScope? parentScope = sp.GetRequiredService<IServiceScopeHierarchy>().ParentScope;
            if (parentScope is null)
                return new WinUIDialogActivationService(sp.GetRequiredService<IDialogProvider>(), sp.GetRequiredService<IWindowService>());
            else
                return parentScope.GetRootScope().ServiceProvider.GetRequiredService<IDialogActivationService<ContentDialogResult>>();
        });

        // Configure INavigationService
        Services.AddScoped<INavigationService, WinUINavigationService>();

        // Configure IPageProvider
        foreach (var (key, descriptor) in Pages.RegisteredPages)
        {
            Services.AddTransient(descriptor.PageType);
            if (descriptor.ViewModelType is not null)
                Services.AddTransient(descriptor.ViewModelType);
        }
        Services.AddSingleton<IPageProvider, WinUIPageProvider>(Pages.Build);
    }

    #region Forward IHostApplicationBuilder members

    IDictionary<object, object> IHostApplicationBuilder.Properties => ((IHostApplicationBuilder)_hostApplicationBuilder).Properties;
    public IConfigurationManager Configuration => _hostApplicationBuilder.Configuration;
    public IHostEnvironment Environment => _hostApplicationBuilder.Environment;
    public ILoggingBuilder Logging => _hostApplicationBuilder.Logging;
    public IMetricsBuilder Metrics => _hostApplicationBuilder.Metrics;
    public IServiceCollection Services => _hostApplicationBuilder.Services;
    void IHostApplicationBuilder.ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure)
        => _hostApplicationBuilder.ConfigureContainer(factory, configure);

    #endregion

    #region IHostApplicationBuilder member configuration methods

    public WinUIApplicationBuilder<TApplication> ConfigureAppConfiguration(Action<IConfigurationManager> action)
    {
        action(Configuration);
        return this;
    }

    public WinUIApplicationBuilder<TApplication> ConfigureEnvironment(Action<IHostEnvironment> action)
    {
        action(Environment);
        return this;
    }

    public WinUIApplicationBuilder<TApplication> ConfigureLogging(Action<ILoggingBuilder> action)
    {
        action(Logging);
        return this;
    }

    public WinUIApplicationBuilder<TApplication> ConfigureMetrics(Action<IMetricsBuilder> action)
    {
        action(Metrics);
        return this;
    }

    public WinUIApplicationBuilder<TApplication> ConfigureServices(Action<IServiceCollection> action)
    {
        action(Services);
        return this;
    }

    #endregion

    #region Extended WinUI services configuration methods

    public WinUIApplicationBuilder<TApplication> UseExtendedWinUIServices()
    {
        _useExtendedWinUIServices = true;
        return this;
    }

    public WinUIApplicationBuilder<TApplication> ConfigurePages()
    {
        // Add pages and view models to DI
        return this;
    }

    public WinUIApplicationBuilder<TApplication> ConfigureWindows(Action<WinUIActivationServiceBuilder> action)
    {
        action(Windows);
        return this;
    }

    #endregion
}