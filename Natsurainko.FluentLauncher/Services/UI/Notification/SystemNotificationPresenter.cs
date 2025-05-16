using FluentLauncher.Infra.UI.Notification;
using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class SystemNotificationPresenter : INotificationPresenter<AppNotification>
{
    private readonly AppNotificationManager _notificationManager = AppNotificationManager.Default;
    private readonly Dictionary<INotification, AppNotification> _appNotifications = [];

    void INotificationPresenter.Clear() => ClearAsync();

    void INotificationPresenter<AppNotification>.Show(INotification<AppNotification> notification) => ShowAsync(notification);

    void INotificationPresenter<AppNotification>.Close(INotification<AppNotification> notification) => CloseAsync(notification);

    public Task ClearAsync() => Task.Run(async () =>
    {
        await _notificationManager.RemoveAllAsync();

        lock (_appNotifications)
        {
            _appNotifications.Clear();
        }
    });

    public Task ShowAsync(INotification<AppNotification> notification) => Task.Run(() =>
    {
        AppNotification appNotification = notification.ConstructUI();

        lock (_appNotifications)
        {
            _appNotifications.Add(notification, appNotification);
        }

        _notificationManager.Show(appNotification);
    });

    public Task CloseAsync(INotification<AppNotification> notification) => Task.Run(async () =>
    {
        if (_appNotifications.TryGetValue(notification, out var appNotification))
        {
            await _notificationManager.RemoveByIdAsync(appNotification.Id);

            lock (_appNotifications)
            {
                _appNotifications.Remove(notification);
            }
        }
    });
}
