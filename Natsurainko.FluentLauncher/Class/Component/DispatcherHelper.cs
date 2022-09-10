using Natsurainko.FluentLauncher.Shared.Mapping;
using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Natsurainko.FluentLauncher.Class.Component;

internal static class DispatcherHelper
{
    public static void RunAsync(DispatchedHandler dispatchedHandler,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = -1)
    {
        Task.Run(async () =>
        {
            var result = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(default, dispatchedHandler);
            await result;

            if (result.Status == Windows.Foundation.AsyncStatus.Error)
            {
                var message = new StringBuilder()
                    .AppendLine("发生在 UI 线程上的未经捕获的异常：")
                    .AppendLine(result.ErrorCode.Message)
                    .AppendLine($"[{memberName}], {sourceFilePath}, {sourceLineNumber}")
                    .AppendLine("原始 Exception: ")
                    .AppendLine(result.ErrorCode.ToString());

                Application.ExceptionWrite(message.ToString());
            }
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                var message = new StringBuilder()
                    .AppendLine("发生在 UI 线程上的未经捕获的异常：")
                    .AppendLine(task.Exception.Message)
                    .AppendLine($"[{memberName}], {sourceFilePath}, {sourceLineNumber}")
                    .AppendLine("原始 Exception: ")
                    .AppendLine(task.Exception.ToString());

                Application.ExceptionWrite(message.ToString());
            }
        });
    }
}
