using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Navigation;

/// <summary>
/// A service that controls the navigation in a window or page
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// The window or page that provides navigation
    /// </summary>
    INavigationProvider NavigationProvider { get; }
    bool CanGoBack { get; }
    bool CanGoForward { get; }

    void GoBack();
    void GoForward();
    void NavigateTo(string key);

    /// <summary>
    /// Called after the navigation provider is initialized
    /// </summary>
    /// <param name="navigationProvider">The window or page that provides navigation</param>
    void InitializeNavigation(INavigationProvider navigationProvider);
}
