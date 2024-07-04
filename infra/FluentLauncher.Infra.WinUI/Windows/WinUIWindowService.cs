using FluentLauncher.Infra.UI.Windows;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Services.UI.Windowing;

/// <summary>
/// Default implementation of <see cref="IActivationService"/> for a WinUI window.
/// </summary>
public class WinUIWindowService : IWindowService
{
    private readonly Window _window;

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }

    public WinUIWindowService(Window window)
    {
        _window = window;
    }

    public void Close() => _window.Close();
    public void Hide() => _window.Hide();
    public void Activate() => _window.Activate();
}
