using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;

#nullable enable

namespace Natsurainko.FluentLauncher.Services.UI.Navigation;

public class NavigationService : INavigationService
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

    public NavigationService(IPageProvider pageProvider)
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
    public void GoBack() => Frame.GoBack();
    public void GoForward() => Frame.GoForward();

    public void NavigateTo(string key, object? parameter = null)
    {
        // Before navigation starts
        if ((Frame.Content as Page)?.DataContext is INavigationAware vmBefore)
            vmBefore.OnNavigatedFrom();

        // Navigation
        var pageInfo = _pageProvider.RegisteredPages[key];
        Frame.Navigate(pageInfo.PageType);

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
            
            if (page.ReadLocalValue(Page.DataContextProperty) != DependencyProperty.UnsetValue && // Requires VM set for the page, rather than inherited
                page.DataContext is INavigationAware vmAfter)
                vmAfter.OnNavigatedTo(parameter);
        }
    }

    #endregion
}