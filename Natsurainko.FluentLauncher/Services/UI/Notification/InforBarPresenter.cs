using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class InfoBarPresenter : INotificationPresenter<InfoBar>
{
    private StackPanel? _itemsContainer;
    private ThemeShadow? _themeShadow;
    private readonly Dictionary<INotification, InfoBar> _infoBars = [];

    void INotificationPresenter.Clear() => ClearAsync();

    void INotificationPresenter<InfoBar>.Show(INotification<InfoBar> notification) => ShowAsync(notification);

    void INotificationPresenter<InfoBar>.Close(INotification<InfoBar> notification) => CloseAsync(notification);

    public Task CloseAsync(INotification<InfoBar> notification) => App.DispatcherQueue.EnqueueAsync(() =>
    {
        EnsureInitialized();

        if (_infoBars.TryGetValue(notification, out var infoBar))
        {
            infoBar.IsOpen = false;
            _itemsContainer.Children.Remove(infoBar);

            lock (_infoBars)
            {
                _infoBars.Remove(notification);
            }
        }
    });

    public Task ShowAsync(INotification<InfoBar> notification) => App.DispatcherQueue.EnqueueAsync(() =>
    {
        EnsureInitialized();

        InfoBar infoBar = notification.ConstructUI();
        infoBar.Shadow = _themeShadow;
        infoBar.CloseButtonClick += (_, _) => CloseAsync(notification);

        lock (_infoBars)
        {
            _infoBars.Add(notification, infoBar);
        }

        _itemsContainer.Children.Add(infoBar);

        if (!double.IsNaN(notification.Delay))
            Task.Delay(TimeSpan.FromSeconds(notification.Delay)).ContinueWith(t => CloseAsync(notification));
    });

    public Task ClearAsync() => App.DispatcherQueue.EnqueueAsync(() =>
    {
        EnsureInitialized();

        lock (_infoBars)
        {
            foreach (var infoBar in _infoBars.Values)
            {
                infoBar.IsOpen = false;
                _itemsContainer.Children.Remove(infoBar);
            }

            _infoBars.Clear();
        }
    });

    public void InitializeContainer(StackPanel stackPanel)
    {
        _themeShadow = new ThemeShadow();
        _itemsContainer = stackPanel;
    }

    [MemberNotNull(nameof(_itemsContainer))]
    private void EnsureInitialized()
    {
        if (_itemsContainer == null)
            throw new InvalidOperationException("ItemsContainer is not initialized.");
    }
}
