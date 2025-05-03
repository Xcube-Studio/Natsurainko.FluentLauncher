using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Utils;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class Notification : INotification, 
    INotification<InfoBar>,
    INotification<TeachingTip>,
    INotification<AppNotification>
{
    public NotificationType Type { get; init; } = NotificationType.Info;

    public required string Title { get; init; }

    public string? Message { get; init; }

    public bool IsClosable { get; init; } = true;

    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(5);

    InfoBar INotification<InfoBar>.ConstructUI()
    {
        return new InfoBar()
        {
            Title = Title,
            Message = Message,
            IsOpen = true,
            IsClosable = IsClosable,
            Translation = new System.Numerics.Vector3(0, 0, 16),
            Severity = Type switch
            {
                NotificationType.Info => InfoBarSeverity.Informational,
                NotificationType.Warning => InfoBarSeverity.Warning,
                NotificationType.Error => InfoBarSeverity.Error,
                NotificationType.Success => InfoBarSeverity.Success,
                _ => InfoBarSeverity.Informational
            }
        };
    }

    TeachingTip INotification<TeachingTip>.ConstructUI()
    {
        return new TeachingTip();
    }

    AppNotification INotification<AppNotification>.ConstructUI()
    {
        return new AppNotificationBuilder()
            .AddText("Send a message.")
            .AddTextBox("textBox")
            .AddButton(new AppNotificationButton("Send")
                .AddArgument("action", "sendMessage")).BuildNotification(); ;
    }
}

internal class ExceptionNotification : INotification,
    INotification<InfoBar>
{
    public NotificationType Type { get; init; } = NotificationType.Error;

    public required string Title { get; init; }

    public string? Message { get; init; }

    public bool IsClosable { get; init; } = true;

    public TimeSpan Delay { get; init; } = TimeSpan.MaxValue;

    public required Exception Exception { get; init; }

    InfoBar INotification<InfoBar>.ConstructUI()
    {
        Button copyButton = new();
        copyButton.Click += Copy;
        copyButton.Content = LocalizedStrings.Notifications_ExceptionCopyButton_Content;

        return new InfoBar()
        {
            Title = Title,
            Message = Message,
            IsOpen = true,
            IsClosable = IsClosable,
            Translation = new System.Numerics.Vector3(0, 0, 16),
            ActionButton = copyButton,
            Severity = Type switch
            {
                NotificationType.Info => InfoBarSeverity.Informational,
                NotificationType.Warning => InfoBarSeverity.Warning,
                NotificationType.Error => InfoBarSeverity.Error,
                NotificationType.Success => InfoBarSeverity.Success,
                _ => InfoBarSeverity.Informational
            }
        };
    }

    private void Copy(object sender, RoutedEventArgs routedEventArgs)
    {
        DataPackage dataPackage = new();
        dataPackage.SetText(string.Join("\r\n", [Title, Message, Exception?.ToString()]));
        Clipboard.SetContent(dataPackage);
    }
}
