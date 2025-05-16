using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.Windows.Input;

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
        InfoBar infoBar = new()
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

        infoBar.Closing += (_, _) => infoBar.IsOpen = true;

        return infoBar;
    }

    TeachingTip INotification<TeachingTip>.ConstructUI()
    {
        return new TeachingTip()
        {
            Title = Title,
            Subtitle = Message,
            CloseButtonContent = LocalizedStrings.Buttons_Confirm_Text,
            PreferredPlacement = TeachingTipPlacementMode.Auto,
            IsLightDismissEnabled = true,
            PlacementMargin = new Thickness(48),
        };
    }

    AppNotification INotification<AppNotification>.ConstructUI()
    {
        return new AppNotificationBuilder()
            .AddText(Title)
            .AddText(Message)
            .BuildNotification();
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

internal class ConfirmNotification : INotification,
    INotification<TeachingTip>
{
    public NotificationType Type { get; init; } = NotificationType.Error;

    public required string Title { get; init; }

    public string? Message { get; init; }

    public bool IsClosable { get; init; } = true;

    public double Delay { get; init; } = double.NaN;

    public object? ActionButtonContent { get; init; } = LocalizedStrings.Buttons_Confirm_Text;

    public Style ActionButtonStyle { get; init; } = (App.Current.Resources["AccentButtonStyle"] as Style)!;

    public ICommand? ActionButtonCommand { get; init; }

    public object? ActionButtonCommandParameter { get; init; }

    public FrameworkElement? Target { get; init; }

    TeachingTip INotification<TeachingTip>.ConstructUI() => new()
    {
        Title = Title,
        Subtitle = Message,
        ActionButtonStyle = ActionButtonStyle,
        ActionButtonContent = ActionButtonContent,
        ActionButtonCommand = ActionButtonCommand,
        ActionButtonCommandParameter = ActionButtonCommandParameter,
        CloseButtonContent = LocalizedStrings.Buttons_Cancel_Text,
        PreferredPlacement = TeachingTipPlacementMode.Auto,
        IsLightDismissEnabled = true,
        PlacementMargin = new Thickness(48),
        Target = Target
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
