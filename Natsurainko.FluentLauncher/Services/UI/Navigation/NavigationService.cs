using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public NavigationService(IPageProvider pageProvider)
    {
        _pageProvider = pageProvider;
    }

    private Frame Frame => _navigationProvider?.NavigationControl as Frame ?? 
        throw new InvalidOperationException("NavigationService has not been initialized");

    public void InitializeNavigation(INavigationProvider navigationProvider)
    {
        NavigationProvider = navigationProvider;
    }

    #region Navigation

    public bool CanGoBack => Frame.CanGoBack;
    public bool CanGoForward => Frame.CanGoForward;
    public void GoBack() => Frame.GoBack();
    public void GoForward() => Frame.GoForward();

    public void NavigateTo(string key)
    {
        var pageInfo = _pageProvider.RegisteredPages[key];
        var vm = _pageProvider.GetViewModel(key);
        Frame.Navigate(pageInfo.PageType);

        if (Frame.Content is Page page)
            page.DataContext = vm;
    }

    #endregion
}