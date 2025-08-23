using FluentLauncher.Infra.UI.Notification;
using System.Collections.Generic;
using System.Linq;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class NotificationService(
    InfoBarPresenter infoBarPresenter,
    SystemNotificationPresenter systemNotificationPresenter,
    TeachingTipPresenter teachingTipPresenter) : INotificationService
{
    public List<INotificationPresenter> Presenters { get; } =
    [
        infoBarPresenter,
        systemNotificationPresenter,
        teachingTipPresenter
    ];

    public void Show<TElement>(INotification<TElement> notification) => Presenters
        .Where(p => p is INotificationPresenter<TElement>)
        .Cast<INotificationPresenter<TElement>>()
        .First()
        .Show(notification);
}
