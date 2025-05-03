using FluentLauncher.Infra.UI.Notification;
using Microsoft.Windows.AppNotifications;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class SystemNotificationPresenter : INotificationPresenter<AppNotification>
{
    private readonly AppNotificationManager _notificationManager = AppNotificationManager.Default;
    private readonly Dictionary<INotification, AppNotification> _appNotifications = [];

    void INotificationPresenter.Clear()
    {

    }

    void INotificationPresenter<AppNotification>.Show(INotification<AppNotification> notification)
    {

    }
    
    void INotificationPresenter<AppNotification>.Close(INotification<AppNotification> notification)
    {

    }
}
