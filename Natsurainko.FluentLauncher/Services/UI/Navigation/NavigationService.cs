using Microsoft.Extensions.DependencyInjection;
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

    private readonly IPageProvider _pageProvider;

    private IServiceScope? _scope;

    public IServiceScope Scope
    {
        get => _scope ?? throw new InvalidOperationException("NavigationService has not been initialized");
        private set => _scope = value;
    }

    public NavigationService(IPageProvider pageProvider)
    {
        _pageProvider = pageProvider;
    }

    private Frame Frame => _navigationProvider?.NavigationControl as Frame ?? 
        throw new InvalidOperationException("NavigationService has not been initialized");

    public void InitializeNavigation(INavigationProvider navigationProvider, IServiceScope scope)
    {
        NavigationProvider = navigationProvider;
        Scope = scope;
    }

    #region Navigation

    public bool CanGoBack => Frame.CanGoBack;
    public bool CanGoForward => Frame.CanGoForward;
    public void GoBack() => Frame.GoBack();
    public void GoForward() => Frame.GoForward();

    public void NavigateTo(string key)
    {
        var pageInfo = _pageProvider.RegisteredPages[key];
        Frame.Navigate(pageInfo.PageType);

        if (Frame.Content is Page page)
        {
            if (Frame.Content is INavigationProvider navPage)
            {
                // Create subscope
                var subScope = Scope.ServiceProvider.CreateScope();

                // Configures VM
                if (pageInfo.ViewModelType is not null)
                    page.DataContext = subScope.ServiceProvider.GetRequiredService(pageInfo.ViewModelType);

                // Configure sub navigation service
                INavigationService subNavService = subScope.ServiceProvider.GetRequiredService<INavigationService>();
                subNavService.InitializeNavigation(navPage, subScope);
                navPage.Initialize(subNavService);
                if (navPage.DefaultPageKey is not null)
                    subNavService.NavigateTo(navPage.DefaultPageKey);
            }
            else
            {
                // Configures VM
                page.DataContext = _pageProvider.GetViewModel(key);
            }
        }
    }

    #endregion
}