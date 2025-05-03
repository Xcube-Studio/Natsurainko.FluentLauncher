using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class TeachingTipPresenter : INotificationPresenter<TeachingTip>
{
    void INotificationPresenter.Clear()
    {

    }

    void INotificationPresenter<TeachingTip>.Show(INotification<TeachingTip> notification)
    {

    }

    void INotificationPresenter<TeachingTip>.Close(INotification<TeachingTip> notification)
    {

    }
}
