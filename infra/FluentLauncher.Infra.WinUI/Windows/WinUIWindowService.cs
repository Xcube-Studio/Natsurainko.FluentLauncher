using FluentLauncher.Infra.UI.Windows;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace FluentLauncher.Infra.WinUI.Windows;

/// <summary>
/// Default implementation of <see cref="IActivationService"/> for a WinUI window.
/// </summary>
public class WinUIWindowService : IWindowService
{
    public Window Window { get; private set; } = null!;

    public string Title
    {
        get => Window.Title;
        set => Window.Title = value;
    }

    public WinUIWindowService() { }

    public void Close() => Window.Close();

    public void Minimize() => Window.Hide();

    public void Activate() => Window.Activate();

    public void InitializeService(Window window)
    {
        Window = window;
    }
}
