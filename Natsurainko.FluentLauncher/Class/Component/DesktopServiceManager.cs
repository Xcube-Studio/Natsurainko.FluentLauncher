using Natsurainko.FluentLauncher.Shared.Desktop;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Popups;

namespace Natsurainko.FluentLauncher.Class.Component;

public static class DesktopServiceManager
{
    static DesktopServiceManager()
    {
        Service = new DesktopService();
        Service.AppServiceDisconnected += Service_AppServiceDisconnected;

        InitializeTask = Service.InitializeAsync();
    }

    public static Task InitializeTask { get; private set; }

    public static DesktopService Service { get; private set; }

    public static void OnBackgroundActivated(BackgroundActivatedEventArgs args) => Service.OnBackgroundActivated(args);

    private static void Service_AppServiceDisconnected(object sender, EventArgs e)
    {
        DispatcherHelper.RunAsync(async () =>
        {
            if (!Service.IsBackground)
            {
                var stringBuilder = new StringBuilder()
                    .AppendLine("应用程序进程 Natsurainko.FluentLauncher.Desktop.exe 意外退出")
                    .AppendLine()
                    .AppendLine("可能的原因：")
                    .AppendLine("1. 进程被其他进程强制关闭")
                    .AppendLine("2. 你的电脑上没有 .NET 6 运行时，导致进程无法启动")
                    .AppendLine("3. 引发的应用程序内部错误");

                var dialog = new MessageDialog(stringBuilder.ToString(), "程序遇到不可恢复的问题");
                dialog.Commands.Add(new UICommand("退出", (ui) => App.Current.Exit()));
                dialog.Commands.Add(new UICommand(".NET 6 运行时", async (ui) =>
                {
                    await Launcher.LaunchUriAsync(new Uri("https://dotnet.microsoft.com/zh-cn/download/dotnet/6.0"));
                    App.Current.Exit();
                }));
                await dialog.ShowAsync();
            }
        });
    }
}
