using FluentLauncher.Infra.UI.Windows;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace FluentLauncher.Infra.WinUI.Windows;

/// <summary>
/// Default implementation of <see cref="IActivationService"/> for a WinUI window.
/// </summary>
public class WinUIWindowService : IWindowService
{
    private Window _window = null!;

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }

    public WinUIWindowService() { }

    public void Close() => _window.Close();

    public void Minimize() => _window.Hide();

    public void Activate() => _window.Activate();

    public void InitializeService(Window window)
    {
        _window = window;
    }
}
