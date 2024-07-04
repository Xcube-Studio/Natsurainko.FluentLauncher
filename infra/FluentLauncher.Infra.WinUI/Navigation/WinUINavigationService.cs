using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Navigation;

public class WinUINavigationService : INavigationService
{
    private INavigationProvider? _navigationProvider;
    public INavigationProvider NavigationProvider
    {
        get => _navigationProvider ?? throw new InvalidOperationException("NavigationService has not been initialized");
        private set => _navigationProvider = value;
    }

    private IServiceScope? _scope;
    public IServiceScope Scope
    {
        get => _scope ?? throw new InvalidOperationException("NavigationService has not been initialized");
        private set => _scope = value;
    }

    private readonly IPageProvider _pageProvider;

    public WinUINavigationService(IPageProvider pageProvider)
    {
        _pageProvider = pageProvider;
    }

    private Frame Frame => _navigationProvider?.NavigationControl as Frame ??
        throw new InvalidOperationException("NavigationService has not been initialized");

    public void InitializeNavigation(INavigationProvider navigationProvider, IServiceScope scope, INavigationService? parent)
    {
        NavigationProvider = navigationProvider;
        Scope = scope;
        Parent = parent;
    }

    #region Navigation

    public INavigationService? Parent { get; private set; }
    public bool CanGoBack => Frame.CanGoBack;
    public bool CanGoForward => Frame.CanGoForward;

    Stack<(string key, object? param)> _backStack = new();
    (string key, object? param) _current;
    Stack<(string key, object? param)> _forwardStack = new();

    public void GoBack()
    {
        Frame.GoBack();
        _forwardStack.Push(_current);
        _current = _backStack.Pop();

        ConfigureFrameContent(_current.key, _current.param);
    }

    public void GoForward()
    {
        Frame.GoForward();
        _backStack.Push(_current);
        _current = _forwardStack.Pop();

        ConfigureFrameContent(_current.key, _current.param);
    }

    public void NavigateTo(string key, object? parameter = null)
    {
        // Before navigation starts
        if ((Frame.Content as Page)?.DataContext is INavigationAware vmBefore)
            vmBefore.OnNavigatedFrom();

        // Navigation
        var pageInfo = _pageProvider.RegisteredPages[key];
        Frame.Navigate(pageInfo.PageType, parameter); // Also forward navigation param to the Page to support the built-in navigation mechanism
        _backStack.Push(_current);
        _forwardStack.Clear();
        _current = (key, parameter);

        ConfigureFrameContent(key, parameter);
    }

    private void ConfigureFrameContent(string key, object? parameter)
    {
        var pageInfo = _pageProvider.RegisteredPages[key];

        if (Frame.Content is Page page)
        {
            if (Frame.Content is INavigationProvider navPage)
            {
                // Create subscope
                var subScope = Scope.ServiceProvider.CreateScope();

                // Configure sub navigation service
                INavigationService subNavService = subScope.ServiceProvider.GetRequiredService<INavigationService>();
                subNavService.InitializeNavigation(navPage, subScope, this);

                // Configures VM in the subscope (after navigation service is initialized)
                if (pageInfo.ViewModelType is not null)
                    page.DataContext = subScope.ServiceProvider.GetRequiredService(pageInfo.ViewModelType);
            }
            else
            {
                // Configures VM
                if (pageInfo.ViewModelType is not null)
                    page.DataContext = Scope.ServiceProvider.GetRequiredService(pageInfo.ViewModelType);
            }

            // After navigation
            if (page.ReadLocalValue(FrameworkElement.DataContextProperty) != DependencyProperty.UnsetValue && // Requires VM set for the page, rather than inherited
                page.DataContext is INavigationAware vmAfter)
                vmAfter.OnNavigatedTo(parameter);
        }
    }

    #endregion
}