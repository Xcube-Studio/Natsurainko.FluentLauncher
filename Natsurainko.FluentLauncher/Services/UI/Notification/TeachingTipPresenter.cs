using CommunityToolkit.WinUI;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class TeachingTipPresenter : INotificationPresenter<TeachingTip>
{
    private readonly Dictionary<INotification, TeachingTip> _teachingTips = [];

    void INotificationPresenter.Clear() => ClearAsync();

    void INotificationPresenter<TeachingTip>.Show(INotification<TeachingTip> notification) => ShowAsync(notification);

    void INotificationPresenter<TeachingTip>.Close(INotification<TeachingTip> notification) => CloseAsync(notification);

    public Task CloseAsync(INotification<TeachingTip> notification) => App.DispatcherQueue.EnqueueAsync(async () =>
    {
        if (_teachingTips.TryGetValue(notification, out var teachingTip))
        {
            lock (_teachingTips)
            {
                _teachingTips.Remove(notification);
            }

            teachingTip.IsOpen = false;

            await Task.Delay(3000);
            ((Grid)App.MainWindow.Content).Children.Remove(teachingTip);
        }
    });

    public Task ShowAsync(INotification<TeachingTip> notification) => App.DispatcherQueue.EnqueueAsync(() =>
    {
        TeachingTip teachingTip = notification.ConstructUI();
        teachingTip.CloseButtonClick += (_, _) => CloseAsync(notification);
        teachingTip.ActionButtonClick += (_, _) => CloseAsync(notification);

        ((Grid)App.MainWindow.Content).Children.Add(teachingTip);

        teachingTip.IsOpen = true;

        lock (_teachingTips)
        {
            _teachingTips.Add(notification, teachingTip);
        }

        if (!double.IsNaN(notification.Delay))
            Task.Delay(TimeSpan.FromSeconds(notification.Delay)).ContinueWith(t => CloseAsync(notification));
    });

    public Task ClearAsync() => App.DispatcherQueue.EnqueueAsync(() =>
    {
        lock (_teachingTips)
        {
            _teachingTips.Clear();
        }
    });
}
