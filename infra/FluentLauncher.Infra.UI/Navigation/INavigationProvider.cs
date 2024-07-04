namespace FluentLauncher.Infra.UI.Navigation;

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
