using Microsoft.Extensions.DependencyInjection;

namespace FluentLauncher.Infra.UI.Navigation;

/// <summary>
/// A service that controls the navigation in a window or page
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// The window or page that provides navigation
    /// </summary>
    INavigationProvider NavigationProvider { get; }

    /// <summary>
    /// Navigation service of the parent window or page if it supports navigation
    /// </summary>
    INavigationService? Parent { get; }

    bool CanGoBack { get; }
    bool CanGoForward { get; }
    void GoBack();
    void GoForward();
    void NavigateTo(string key, object? parameter = null);
}
