using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Windows;
using FluentLauncher.Infra.WinUI.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.WinUI.Dialogs;

class WinUIDialogActivationService : IDialogActivationService<ContentDialogResult>
{
    private readonly IDialogProvider _dialogProvider;
    private readonly IWindowService _windowService;

    private readonly XamlRoot _xamlRoot;

    public WinUIDialogActivationService(IDialogProvider dialogProvider, IWindowService windowService)
    {
        _dialogProvider = dialogProvider;
        _windowService = windowService;

        _xamlRoot = ((WinUIWindowService)windowService).Window.Content.XamlRoot;
    }

    public Task<ContentDialogResult> ShowAsync(string key)
    {
        var dialog = (ContentDialog)_dialogProvider.GetDialog(key);
        dialog.XamlRoot = _xamlRoot;

        if (((WinUIWindowService)_windowService).Window.Content is FrameworkElement frameworkElement)
            dialog.RequestedTheme = frameworkElement.RequestedTheme;

        return dialog.ShowAsync().AsTask();
    }

    public Task<ContentDialogResult> ShowAsync(string key, object param)
    {
        var dialog = (ContentDialog)_dialogProvider.GetDialog(key);
        dialog.XamlRoot = _xamlRoot;

        if (((WinUIWindowService)_windowService).Window.Content is FrameworkElement frameworkElement)
            dialog.RequestedTheme = frameworkElement.RequestedTheme;

        if (dialog.DataContext is IDialogParameterAware vm)
            vm.HandleParameter(param);

        return dialog.ShowAsync().AsTask();
    }
}
