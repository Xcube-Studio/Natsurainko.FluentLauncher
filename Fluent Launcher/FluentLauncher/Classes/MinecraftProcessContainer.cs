using FluentLauncher.Models;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;

namespace FluentLauncher.Classes
{
    public class MinecraftProcessContainer
    {
        public MinecraftProcessContainer()
        {

        }

        public event EventHandler WaitForInputIdle;

        public event EventHandler Exited;

        public event EventHandler Crashed;

        public event EventHandler<ProcessOutput> OutputReceived;

        public event EventHandler<ProcessOutput> ErrorOutputReceived;

        public event EventHandler<string> DownloadInfoReceived;

        public event EventHandler<string> InfoReceived;

        public bool IsRunning { get; set; } = false;

        public string ProcessInfo { get; set; }

        public string McCore { get; set; }

        public async void Launch()
        {
            await ShareResource.SetDownloadSource();

            IsRunning = true;
            McCore = ShareResource.SelectedCore.Id;

            App.DesktopBridge.Connection.RequestReceived += Connection_RequestReceived;

            await App.DesktopBridge.SendAsync<StandardResponseModel>(new LaunchMinecraftRequest());
        }

        private void Connection_RequestReceived(Windows.ApplicationModel.AppService.AppServiceConnection sender, Windows.ApplicationModel.AppService.AppServiceRequestReceivedEventArgs args)
        {
            if ((string)args.Request.Message["Header"] != "MinecraftLauncherDetails")
                return;

            var res = JsonConvert.DeserializeObject<StandardResponseModel>(JsonConvert.SerializeObject(args.Request.Message));

            switch (res.Message)
            {
                case "Info":
                    _ = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, delegate
                    {
                        InfoReceived?.Invoke(null, res.Response);
                    });
                    break;
                case "DownloadInfo":
                    _ = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, delegate
                    {
                        DownloadInfoReceived?.Invoke(null, res.Response);
                    });
                    break;
                case "Event":
                    HandeleEventTrigger(res.Response);
                    break;
                case "OutputReceived":
                    OutputReceived?.Invoke(this, GetProcessOutput(res.Response));
                    break;
                case "ErrorOutputReceived":
                    ErrorOutputReceived?.Invoke(this, GetProcessOutput(res.Response));
                    break;
                case "Process":
                    ProcessInfo = res.Response;
                    break;
                default:
                    break;
            }
        }

        private void HandeleEventTrigger(string eventName)
        {
            switch (eventName)
            {
                case "Exited":
                    Exited?.Invoke(this, null);
                    App.DesktopBridge.Connection.RequestReceived -= Connection_RequestReceived;
                    break;
                case "WaitForInputIdle":
                    WaitForInputIdle?.Invoke(this, null);
                    break;
                case "Crashed":
                    Crashed?.Invoke(this, null);
                    break;
                default:
                    break;
            }
        }

        private ProcessOutput GetProcessOutput(string line)
        {
            var regex = new Regex(@"(?i)(?<=\[)(.*)(?=\])");
            var output = new ProcessOutput() { Content = line };

            foreach (Match result in regex.Matches(line))
            {
                if (result.Value.ToLower().Contains("info"))
                {
                    output.Type = "INFO";
                    break;
                }
                else if (result.Value.ToLower().Contains("fatal"))
                {
                    output.Type = "FATAL";
                    break;
                }
                else if (result.Value.ToLower().Contains("debug"))
                {
                    output.Type = "DEBUG";
                    break;
                }
                else if (result.Value.ToLower().Contains("warn"))
                {
                    output.Type = "WARN";
                    break;
                }
                else if (result.Value.ToLower().Contains("error"))
                {
                    output.Type = "ERROR";
                    break;
                }
            }

            if (line.StartsWith('	'))
                output.Type = "ERROR";

            return output;
        }
    }
}
