using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.ApplicationModel.Activation;

namespace FluentLauncher.DesktopBridger
{
    public class AppDesktopBridgeContainer
    {
        public BackgroundTaskDeferral TaskDeferral { get; set; }

        public AppServiceConnection Connection { get; set; }

        public bool IsBackground { get; set; } = false;

        public event EventHandler<AppServiceTriggerDetails> AppServiceConnected;

        public event EventHandler AppServiceDisconnected;

        public async Task BeginInitAsync()
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            await Task.Delay(1000);
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
                    this.AppServiceDisconnected += AppDesktopBridgeContainer_AppServiceDisconnected;
                }
            }
        }

        private async void AppDesktopBridgeContainer_AppServiceDisconnected(object sender, EventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (!IsBackground)
                {
                    var dialog = new MessageDialog("Connection to desktop process lost, Reconnect?");
                    dialog.Commands.Add(new UICommand("Yes", async (ui) => await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync()));
                    dialog.Commands.Add(new UICommand("No", (ui) => App.Current.Exit()));
                    await dialog.ShowAsync();
                }
            });
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            TaskDeferral?.Complete();
            TaskDeferral = null;
            Connection?.Dispose();
            Connection = null;
            AppServiceDisconnected?.Invoke(this, null);
            this.AppServiceDisconnected -= AppDesktopBridgeContainer_AppServiceDisconnected;
        }

        public async Task<T> SendAsync<T>(IDesktopMessage message)
        {
            string value = JsonConvert.SerializeObject(message);
            var valueSet = JsonConvert.DeserializeObject<ValueSet>(value);
            var res = JsonConvert.SerializeObject((await this.Connection.SendMessageAsync(valueSet)).Message);
            return JsonConvert.DeserializeObject<T>(res);
        }
    }
}
