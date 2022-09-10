using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Shared.Desktop;

public class DesktopService
{
    public BackgroundTaskDeferral TaskDeferral { get; set; }

    public AppServiceConnection Connection { get; set; }

    public bool IsBackground { get; set; } = false;

    public event EventHandler<AppServiceTriggerDetails> AppServiceConnected;

    public event EventHandler AppServiceDisconnected;

    public event EventHandler<ValueSet> RequestReceived;

    public async Task InitializeAsync()
    {
        await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();

        while (Connection == null)
            await Task.Delay(100);

        Connection.RequestReceived += Connection_RequestReceived;
    }

    public void OnBackgroundActivated(BackgroundActivatedEventArgs args)
    {
        if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
        {
            if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
            {
                TaskDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += TaskInstance_Canceled;

                Connection = details.AppServiceConnection;
                AppServiceConnected?.Invoke(this, args.TaskInstance.TriggerDetails as AppServiceTriggerDetails);
            }
        }
    }

    private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
    {
        TaskDeferral?.Complete();
        TaskDeferral = null;
        Connection?.Dispose();
        Connection = null;
        AppServiceDisconnected?.Invoke(this, null);
    }

    private void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        => RequestReceived?.Invoke(sender, args.Request.Message);

    public async Task<MethodResponse> SendAsync(MethodRequest request)
    {
        while (Connection == null)
            await Task.Delay(50);

        return MethodResponse.CreateFromValueSet((await this.Connection.SendMessageAsync(request.CreateValueSet())).Message);
    }

    public async Task<MethodResponse<T>> SendAsync<T>(MethodRequest request)
    {
        while (Connection == null)
            await Task.Delay(50);

        return MethodResponse.CreateFromValueSet<T>((await this.Connection.SendMessageAsync(request.CreateValueSet())).Message);
    }

    public async Task SendAsyncWithoutResponse(MethodRequest request)
    {
        while (Connection == null)
            await Task.Delay(50);

        _ = this.Connection.SendMessageAsync(request.CreateValueSet());
    }
}
