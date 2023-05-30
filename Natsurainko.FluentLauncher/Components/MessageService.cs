using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Natsurainko.FluentCore.Exceptions;
using Natsurainko.FluentLauncher.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Components;

public static class MessageService
{
    private static ListBox _messageList;

    public static void RegisterContainer(ListBox messageList) => _messageList = messageList;

    public static void Show(
        string title,
        string message = "",
        InfoBarSeverity severity = InfoBarSeverity.Informational,
        int delay = 5000,
        ButtonBase button = null)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            if (_messageList == null)
                return;

            var obj = new MessageData
            {
                Button = button,
                Message = message,
                Title = title,
                Severity = severity,
                Removed = false
            };

            _messageList.Items.Add(obj);
            await Task.Delay(delay);

            if (!obj.Removed)
                _messageList.Items.Remove(obj);
        });
    }

    public static void ShowException(Exception exception, string title, int delay = 15000)
    {
        if (exception is MicrosoftAuthenticationException microsoftAuthenticationException)
        {
            var builder = new StringBuilder();
            builder.AppendLine(title);
            builder.AppendLine(microsoftAuthenticationException.Message);
            builder.AppendLine(microsoftAuthenticationException.HelpLink);

            Show(builder.ToString(), microsoftAuthenticationException.StackTrace, InfoBarSeverity.Error, delay);
            return;
        }

        Show(title, exception.ToString(), InfoBarSeverity.Error, delay);
    }

    public static void ShowSuccess(string title, string message = "")
        => Show(title, message, InfoBarSeverity.Success);

    public static void ShowError(string title, string message = "", int delay = 15000)
        => Show(title, message, InfoBarSeverity.Error, delay);
}
