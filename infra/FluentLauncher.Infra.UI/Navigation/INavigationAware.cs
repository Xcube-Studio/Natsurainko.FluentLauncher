namespace FluentLauncher.Infra.UI.Navigation;

/// <summary>
/// Implemented by a view model that can respond to navigation events
/// </summary>
public interface INavigationAware
{
    void OnNavigatedTo(object? parameter) { }

    void OnNavigatedFrom() { }
}
