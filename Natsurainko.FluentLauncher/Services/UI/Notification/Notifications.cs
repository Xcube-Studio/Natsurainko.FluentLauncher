using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Utils;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Services.UI.Notification;

internal class DefaultNotification : INotification, 
    INotification<InfoBar>,
    INotification<TeachingTip>,
    INotification<AppNotification>
{
    public NotificationType Type { get; init; } = NotificationType.Info;

    public required string Title { get; init; }

    public string? Message { get; init; }

    public bool IsClosable { get; init; } = true;

    public double Delay { get; init; } = 5;

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

internal class ActionNotification : INotification,
    INotification<InfoBar>
{
    public NotificationType Type { get; init; } = NotificationType.Error;

    public required string Title { get; init; }

    public string? Message { get; init; }

    public bool IsClosable { get; init; } = true;

    public double Delay { get; init; } = 5;

    public Func<ButtonBase>? GetActionButton { get; set; }

    InfoBar INotification<InfoBar>.ConstructUI() => new InfoBar()
    {
        Title = Title,
        Message = Message,
        IsOpen = true,
        IsClosable = IsClosable,
        Translation = new System.Numerics.Vector3(0, 0, 16),
        ActionButton = GetActionButton?.Invoke(),
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

internal class ExceptionNotification : ActionNotification
{
    public ExceptionNotification()
    {
        this.Delay = 30;
        this.Type = NotificationType.Error;

        this.GetActionButton = () =>
        {
            Button copyButton = new();
            copyButton.Click += (_,_) => ClipboardHepler.SetText(string.Join("\r\n", [Title, Message, Exception?.ToString()]));
            copyButton.Content = LocalizedStrings.Buttons_CopyException_Text;

            return copyButton;
        };
    }

    public required Exception Exception { get; init; }
}
