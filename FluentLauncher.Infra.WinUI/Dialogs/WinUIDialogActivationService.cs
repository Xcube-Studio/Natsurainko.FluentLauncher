using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Windows;
using FluentLauncher.Infra.WinUI.Windows;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace FluentLauncher.Infra.WinUI.Dialogs;

class WinUIDialogActivationService(IDialogProvider dialogProvider, IWindowService windowService) : IDialogActivationService<ContentDialogResult>
{
    private readonly XamlRoot _xamlRoot = ((WinUIWindowService)windowService).Window.Content.XamlRoot;

    public Task<ContentDialogResult> ShowAsync(string key)
    {
        var dialog = (ContentDialog)dialogProvider.GetDialog(key);
        dialog.XamlRoot = _xamlRoot;

        if (((WinUIWindowService)windowService).Window.Content is FrameworkElement frameworkElement)
            dialog.RequestedTheme = frameworkElement.RequestedTheme;

        return dialog.ShowAsync().AsTask();
    }

    public async Task<(ContentDialogResult, TDialogResult?)> ShowAsync<TDialogResult>(string key)
    {
        var dialog = (ContentDialog)dialogProvider.GetDialog(key);
        dialog.XamlRoot = _xamlRoot;

        if (((WinUIWindowService)windowService).Window.Content is FrameworkElement frameworkElement)
            dialog.RequestedTheme = frameworkElement.RequestedTheme;

        if (dialog is not IDialogResultAware<TDialogResult> dialogResultAware)
            throw new InvalidOperationException($"Dialog with key '{key}' does not implement {nameof(IDialogResultAware<TDialogResult>)} interface.");

        return (await dialog.ShowAsync(), dialogResultAware.Result);
    }

    public Task<ContentDialogResult> ShowAsync(string key, object param)
    {
        var dialog = (ContentDialog)dialogProvider.GetDialog(key);
        dialog.XamlRoot = _xamlRoot;

        if (((WinUIWindowService)windowService).Window.Content is FrameworkElement frameworkElement)
            dialog.RequestedTheme = frameworkElement.RequestedTheme;

        if (dialog.DataContext is IDialogParameterAware vm)
            vm.HandleParameter(param);

        return dialog.ShowAsync().AsTask();
    }

    public async Task<(ContentDialogResult, TDialogResult?)> ShowAsync<TDialogResult>(string key, object param)
    {
        var dialog = (ContentDialog)dialogProvider.GetDialog(key);
        dialog.XamlRoot = _xamlRoot;

        if (((WinUIWindowService)windowService).Window.Content is FrameworkElement frameworkElement)
            dialog.RequestedTheme = frameworkElement.RequestedTheme;

        if (dialog.DataContext is IDialogParameterAware vm)
            vm.HandleParameter(param);

        if (dialog is not IDialogResultAware<TDialogResult> dialogResultAware)
            throw new InvalidOperationException($"Dialog with key '{key}' does not implement {nameof(IDialogResultAware<TDialogResult>)} interface.");

        return (await dialog.ShowAsync(), dialogResultAware.Result);
    }
}
