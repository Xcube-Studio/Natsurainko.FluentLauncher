using Microsoft.Extensions.DependencyInjection;

namespace Natsurainko.FluentLauncher.Services.UI.Navigation;

#nullable enable

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

    /// <summary>
    /// Called after the navigation provider is initialized
    /// </summary>
    /// <param name="navigationProvider">The window or page that provides navigation</param>
    /// <param name="scope">The scope of the navigation provider</param>
    // This is needed because IServiceScope cannot be the dependecy of an INavigationService, and
    // every INavigationProvider depends on an INavigationService.
    // Therefore, they can only be set after the navigation provider and its scope are created.
    void InitializeNavigation(INavigationProvider navigationProvider, IServiceScope scope, INavigationService? parent);
}
