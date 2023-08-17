using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Services.UI.Windows;

/// <summary>
/// Default implementation of <see cref="IActivationService"/> for a WinUI window.
/// </summary>
class WindowService : IWindowService
{
    private readonly Window _window;

    public string Title
    {
        get => _window.Title;
        set => _window.Title = value;
    }

    public WindowService(Window window)
    {
        _window = window;
    }

    public void Close() => _window.Close();
    public void Hide() => _window.Hide();
    public void Activate() => _window.Activate();
}
