namespace Natsurainko.FluentLauncher.Services.UI.Navigation;

#nullable enable

/// <summary>
/// A page or window that provides navigation
/// </summary>
public interface INavigationProvider
{
    /// <summary>
    /// The UI element that provides navigation
    /// </summary>
    object NavigationControl { get; }
}
