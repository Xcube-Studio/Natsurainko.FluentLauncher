using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Shared.Desktop;

public class DesktopServiceListener
{
    public static List<Type> Modules { get; private set; } = new List<Type>();

    public AppServiceConnection Connection { get; set; }

    public bool IsConnected { get; set; } = false;

    public event EventHandler<ValueSet> RequestReceived;

    public async Task InitializeAsync()
    {
        Connection = new AppServiceConnection
        {
            AppServiceName = "Natsurainko.FluentLauncher.Desktop",
            PackageFamilyName = Package.Current.Id.FamilyName
        };

        Connection.RequestReceived += Connection_RequestReceived;
        Connection.ServiceClosed += Connection_ServiceClosed;

        while (true)
        {
            var status = await Connection.OpenAsync();

            if (status == AppServiceConnectionStatus.Success)
            {
                IsConnected = true;
                return;
            }
        }
    }

    public void WaitForExited()
    {
        Task.Run(async () =>
        {
            while (IsConnected)
                await Task.Delay(500);
        }).Wait();
    }

    public async void SendResponseAsync(MethodResponse response)
    {
        Console.WriteLine($"[{DateTime.Now:hh:mm:ss}][SendResponseAsync][{response.Method}][{response.ImplementId}][{response.Response}]");
        await Connection.SendMessageAsync(response.CreateValueSet());
    }

    public void BindingModules(List<Type> modules)
        => DesktopServiceListener.Modules = modules;

    private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args) => this.IsConnected = false;

    private async void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
    {
        RequestReceived?.Invoke(sender, args.Request.Message);

        var request = MethodRequest.CreateFromValueSet(args.Request.Message);

        Console.WriteLine($"[{DateTime.Now:hh:mm:ss}][RequestReceived][{request.Method}][{request.ImplementId}]");

        var res = request.Run();

        if (res != null)
        {
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss}][SendResponseAsync][{res.Method}][{res.ImplementId}][{res.Response}]");
            await args.Request.SendResponseAsync(res.CreateValueSet());
        }
    }
}

public static class MethodRequestExtension
{
    public static MethodResponse Run(this MethodRequest methodRequest)
    {
        foreach (var methods in DesktopServiceListener.Modules.Select(x => x.GetMethods()))
            foreach (var method in methods)
                if (method.Name == methodRequest.Method)
                {
                    var response = new MethodResponse
                    {
                        ImplementId = methodRequest.ImplementId,
                        Method = methodRequest.Method
                    };

                    try { response.Response = method.Invoke(null, methodRequest.MethodParameters.Select(x => x.GetValue()).ToArray()); } catch { }

                    return response;
                }

        return null;
    }
}