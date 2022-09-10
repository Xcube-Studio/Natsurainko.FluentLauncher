using Natsurainko.FluentCore.Module.Downloader;
using Natsurainko.FluentCore.Service;
using Natsurainko.FluentLauncher.Shared.Desktop;

#if WINDOWS_UWP
using Natsurainko.FluentLauncher.Class.Component;
#endif

namespace Natsurainko.FluentLauncher.Shared.Mapping;

public class DefaultSettings
{
#if WINDOWS_UWP

    public static void SetDownloadSource(string source)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(source)
            .SetMethod("SetDownloadSource");

        _ = DesktopServiceManager.Service.SendAsync(builder.Build());
    }

    public static void SetMaxDownloadThreads(int number)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(number)
            .SetMethod("SetMaxDownloadThreads");

        _ = DesktopServiceManager.Service.SendAsync(builder.Build());
    }

#endif

#if NETCOREAPP

    public static void SetDownloadSource(string source)
    {
        switch (source)
        {
            case "mojang":
                DownloadApiManager.Current = DownloadApiManager.Mojang;
                break;
            case "bmclapi":
                DownloadApiManager.Current = DownloadApiManager.Bmcl;
                break;
            case "mcbbs":
                DownloadApiManager.Current = DownloadApiManager.Mcbbs;
                break;
            default:
                break;
        }
    }

    public static void SetMaxDownloadThreads(int number)
    {
        if (number > 0)
            ResourceDownloader.MaxDownloadThreads = number;
    }

#endif
}
