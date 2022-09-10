using Natsurainko.FluentLauncher.Shared.Desktop;
using System;

#if WINDOWS_UWP
using Natsurainko.FluentLauncher.Class.Component;
#endif

namespace Natsurainko.FluentLauncher.Shared.Mapping;

public class Application
{
#if WINDOWS_UWP

    public static void ExceptionWrite(string text)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(text)
            .SetMethod("ExceptionWrite");

        _ = DesktopServiceManager.Service.SendAsync(builder.Build());
    }

#endif

#if NETCOREAPP

    public static void ExceptionWrite(string text) => Console.WriteLine(text);

#endif

}
