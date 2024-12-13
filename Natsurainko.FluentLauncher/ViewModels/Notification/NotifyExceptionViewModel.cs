using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text;
using Windows.ApplicationModel.DataTransfer;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Notification;

internal partial class NotifyExceptionViewModel : ObservableObject
{
    public NotifyExceptionViewModel(Exception exception)
    {
        Exception = exception;

        var @string = new StringBuilder();
        @string.AppendLine((Exception.InnerException ?? Exception).GetType().ToString());

        if (!string.IsNullOrEmpty((Exception.InnerException ?? Exception).Message))
            Description += "...\r\n" + (Exception.InnerException ?? Exception).Message;

        if (!string.IsNullOrEmpty((Exception.InnerException ?? Exception).HelpLink))
            Description += "...\r\n" + (Exception.InnerException ?? Exception).HelpLink;

        @string.AppendLine((Exception.InnerException ?? Exception).StackTrace);

        ExceptionMessage = @string.ToString();
    }
    public string ExceptionMessage { get; private set; }

    public Exception Exception { get; private set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    [RelayCommand]
    public void Copy()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText($"{Description}\r\n{ExceptionMessage}");
        Clipboard.SetContent(dataPackage);
    }
}
