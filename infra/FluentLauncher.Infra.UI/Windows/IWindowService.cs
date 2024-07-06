namespace FluentLauncher.Infra.UI.Windows;

// Service for operations of a window
public interface IWindowService
{
    string Title { get; set; }

    /// <summary>
    /// Close the window
    /// </summary>
    void Close();

    /// <summary>
    /// Minimize the window
    /// </summary>
    void Minimize();

    /// <summary>
    /// Activate the window
    /// </summary>
    void Activate();
}
