using FluentLauncher.Infra.UI;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Pages;
using FluentLauncher.Infra.WinUI.Mvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace FluentLauncher.Infra.WinUI.Navigation;

public class WinUINavigationService : INavigationService
{
    private INavigationProvider? _navigationProvider;

    private readonly IPageProvider _pageProvider;
    private readonly IServiceScopeHierarchy _scopeHierarchy;

    private Frame Frame => _navigationProvider?.NavigationControl as Frame
        ?? throw new InvalidOperationException("E001");

    private IServiceScope CurrentScope => _scopeHierarchy.CurrentScope;

    public INavigationProvider NavigationProvider
    {
        get => _navigationProvider ?? throw new InvalidOperationException("E001");
        private set => _navigationProvider = value;
    }

    public INavigationService? Parent
    {
        get
        {
            IServiceScope? parentScope = _scopeHierarchy.ParentScope;
            return parentScope?.ServiceProvider.GetRequiredService<INavigationService>();
        }
    }

    public WinUINavigationService(IPageProvider pageProvider, IServiceScopeHierarchy scopeHierarchy)
    {
        _pageProvider = pageProvider;
        _scopeHierarchy = scopeHierarchy;
    }

    public void InitializeService(INavigationProvider navigationProvider)
    {
        NavigationProvider = navigationProvider;
    }

    public bool CanGoBack => Frame.CanGoBack;

    public bool CanGoForward => Frame.CanGoForward;

    Stack<(string key, object? param)> _backStack = new();
    Stack<(string key, object? param)> _forwardStack = new();
    (string key, object? param) _current;

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
            if (Frame.Content is INavigationProvider navigationPage)
            {
                // Create child scope
                var childScope = CurrentScope.CreateChildScope();

                // Configure navigation service in the child scope
                INavigationService childNavigationService = childScope.ServiceProvider.GetRequiredService<INavigationService>();
                ((WinUINavigationService)childNavigationService).InitializeService(navigationPage);

                // Configures VM in the child scope (after navigation service is initialized)
                if (pageInfo.ViewModelType is not null)
                    page.DataContext = childScope.ServiceProvider.GetRequiredService(pageInfo.ViewModelType);
            }
            else
            {
                // Configures VM
                if (pageInfo.ViewModelType is not null)
                    page.DataContext = CurrentScope.ServiceProvider.GetRequiredService(pageInfo.ViewModelType);
            }

            // After navigation
            if (page.ReadLocalValue(FrameworkElement.DataContextProperty) != DependencyProperty.UnsetValue && // Requires VM set for the page, rather than inherited
                page.DataContext is INavigationAware vmAfter)
                vmAfter.OnNavigatedTo(parameter);

            if (page != null && page.DataContext is IViewAssociated viewAssociatedModel)
            {
                viewAssociatedModel.Dispatcher = page.DispatcherQueue;

                page.Loaded += (_, _) => viewAssociatedModel.OnLoaded();
                page.Unloaded += (_, _) => viewAssociatedModel.OnUnloaded();

                if (viewAssociatedModel is IViewAssociated<Page> pageAssociatedModel)
                    pageAssociatedModel.SetView(page);
            }
        }
    }
}
