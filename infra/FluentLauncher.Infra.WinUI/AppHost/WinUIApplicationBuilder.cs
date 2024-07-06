using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Windows;
using FluentLauncher.Infra.WinUI.Navigation;
using FluentLauncher.Infra.WinUI.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.AppHost;

public class WinUIApplicationBuilder : IHostApplicationBuilder
{
    private readonly HostApplicationBuilder _hostApplicationBuilder;
    private readonly Func<Application> _createApplicationFunc;

    private bool _useExtendedWinUIServices = false;

    public WinUIActivationServiceBuilder Windows { get; } = new();

    public WinUIApplicationBuilder(Func<Application> createApplicationFunc)
    {
        _createApplicationFunc = createApplicationFunc;

        _hostApplicationBuilder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            // TODO: pass parameters from ctor
        });
    }

    public WinUIApplication Build()
    {
        if (_useExtendedWinUIServices)
            ConfigureExtendedWinUIServices();

        IHost host = _hostApplicationBuilder.Build();
        return new WinUIApplication(_createApplicationFunc, host);
    }

    private void ConfigureExtendedWinUIServices()
    {
        // Configure IActivationService
        foreach (var (key, descriptor) in Windows.RegisteredWindows)
        {
            // Always add as scoped for NavigationService; single-instance property is dealt by ActivationService
            Services.AddScoped(descriptor.WindowType);
        }
        Services.AddSingleton<IActivationService, WinUIActivationService>((sp) =>
        {
            return Windows.Build(sp);
        });

        // Configure INavigationService
        Services.AddScoped<INavigationService, WinUINavigationService>();

        // TODO: Configure IPageProvider

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

    public WinUIApplicationBuilder ConfigureAppConfiguration(Action<IConfigurationManager> action)
    {
        action(Configuration);
        return this;
    }

    public WinUIApplicationBuilder ConfigureEnvironment(Action<IHostEnvironment> action)
    {
        action(Environment);
        return this;
    }

    public WinUIApplicationBuilder ConfigureLogging(Action<ILoggingBuilder> action)
    {
        action(Logging);
        return this;
    }

    public WinUIApplicationBuilder ConfigureMetrics(Action<IMetricsBuilder> action)
    {
        action(Metrics);
        return this;
    }

    public WinUIApplicationBuilder ConfigureServices(Action<IServiceCollection> action)
    {
        action(Services);
        return this;
    }

    #endregion

    #region Extended WinUI services configuration methods

    public WinUIApplicationBuilder UseExtendedWinUIServices()
    {
        _useExtendedWinUIServices = true;
        return this;
    }

    public WinUIApplicationBuilder ConfigurePages()
    {
        // Add pages and view models to DI
        return this;
    }

    public WinUIApplicationBuilder ConfigureWindows(Action<WinUIActivationServiceBuilder> action)
    {
        action(Windows);
        return this;
    }

    #endregion
}