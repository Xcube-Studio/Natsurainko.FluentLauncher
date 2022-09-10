using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentCore.Event;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Wrapper;
using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Desktop;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;

namespace Natsurainko.FluentLauncher.Shared.Mapping;

using Natsurainko.FluentCore.Class.Model.Download;
using Natsurainko.FluentCore.Module.Downloader;

#if WINDOWS_UWP
using Natsurainko.FluentLauncher.Class.Component;

public class LauncherProcess
{
    public string JavaRuntime { get; private set; }

    public DateTime ProcessStartTime { get; private set; }

    public GameCore GameCore { get; private set; }

    public CustomLaunchSetting CustomLaunchSetting { get; set; }

    public IEnumerable<string> Arguments { get; private set; }

    public ObservableCollection<string> Outputs { get; private set; }

    public event EventHandler<MinecraftExitedArgs> Exited;

    public event EventHandler<string> ProcessOutput;

    public event EventHandler<string> StateChanged;

    public event EventHandler<JObject> LaunchFailed;

    private Guid ExitedEventId;

    private Guid ProcessOutputEventId;

    private Guid StopMethodId;

    private Guid DownloadProgressChangedEventId;

    private Guid DownloadCompletedEventId;

    private bool Downloaded = false;

    public LauncherProcess(GameCore gameCore)
    {
        JavaRuntime = ConfigurationManager.AppSettings.CurrentJavaRuntime;
        GameCore = gameCore;

        StopMethodId = Guid.NewGuid();
        DownloadProgressChangedEventId = Guid.NewGuid();
        DownloadCompletedEventId = Guid.NewGuid();
    }

    public async Task<bool> LaunchAsync()
    {
        SetState(1);
        SetState(2);

        var downloaderBuilder = MethodRequestBuilder.Create()
            .AddParameter(GameCore.Root.FullName)
            .AddParameter(GameCore.Id)
            .AddParameter(DownloadProgressChangedEventId.ToString())
            .AddParameter(DownloadCompletedEventId.ToString())
            .SetMethod("BeginResourceDownloader");

        DesktopServiceManager.Service.RequestReceived += OnDownloadProgressChanged;
        DesktopServiceManager.Service.RequestReceived += OnDownloadCompleted;

        await DesktopServiceManager.Service.SendAsyncWithoutResponse(downloaderBuilder.Build());

        while (!Downloaded)
            await Task.Delay(100);

        CustomLaunchSetting = await GameCore.GetCustomLaunchSetting();

        var builder = MethodRequestBuilder.Create()
            .AddParameter((CreateDictionary(), "System.Collections.Generic.Dictionary`2[[System.String],[System.String]]"))
            .AddParameter((CreateLaunchSetting(), "Natsurainko.FluentCore.Class.Model.Launch.LaunchSetting, Natsurainko.FluentCore"))
            .SetMethod("LaunchMinecraft");

        SetState(3);
        var res = (await DesktopServiceManager.Service.SendAsync<Dictionary<string, object>>(builder.Build())).Response;

        ExitedEventId = Guid.Parse((string)res["ExitedEventId"]);
        ProcessOutputEventId = Guid.Parse((string)res["ProcessOutputEventId"]);

        switch (int.Parse(res["State"].ToString()))
        {
            case 0:
                SetState(4);
                ProcessStartTime = (DateTime)res["StartTime"];
                Outputs = new();

                DesktopServiceManager.Service.RequestReceived += DesktopService_RequestReceived;
                Arguments = ((JArray)res["Arguemnts"]).Select(x => x.ToString());

                return true;
            case 1:
                SetState(5);
                LaunchFailed?.Invoke(this, (JObject)res["Exception"]);
                break;
            case 2:
                SetState(6);
                break;
        }

        return false;
    }

    public async Task StopProcessAsync()
    {
        var request = new MethodRequest
        {
            ImplementId = StopMethodId,
            Method = "StopProcess"
        };

        await DesktopServiceManager.Service.SendAsyncWithoutResponse(request);
    }

    public LaunchSetting CreateLaunchSetting()
    {
        var launchSetting = new LaunchSetting()
        {
            Account = ConfigurationManager.AppSettings.CurrentAccount,
            JvmSetting = new()
            {
                MaxMemory = ConfigurationManager.AppSettings.JavaVirtualMachineMemory.GetValueOrDefault(1024)
            },
            IsDemoUser = ConfigurationManager.AppSettings.EnableDemoUser.GetValueOrDefault(false),
            EnableIndependencyCore = GameCore.GetEnableIndependencyCore(CustomLaunchSetting)
        };

        if (CustomLaunchSetting.Enable)
        {
            launchSetting.GameWindowSetting = new()
            {
                Width = CustomLaunchSetting.GameWindowWidth,
                Height = CustomLaunchSetting.GameWindowHeight,
                IsFullscreen = CustomLaunchSetting.EnableFullScreen
            };

            if (!string.IsNullOrEmpty(CustomLaunchSetting.GameServerAddress))
            {
                if (CustomLaunchSetting.GameServerAddress.Contains(':'))
                {
                    var address = CustomLaunchSetting.GameServerAddress.Split(':');

                    launchSetting.ServerSetting = new()
                    {
                        IPAddress = address[0],
                        Port = int.Parse(address[1])
                    };
                }
                else launchSetting.ServerSetting = new()
                {
                    IPAddress = CustomLaunchSetting.GameServerAddress,
                    Port = 25565
                };
            }
        }
        else
        {
            launchSetting.GameWindowSetting = new()
            {
                Width = ConfigurationManager.AppSettings.GameWindowWidth.GetValueOrDefault(854),
                Height = ConfigurationManager.AppSettings.GameWindowHeight.GetValueOrDefault(480),
                IsFullscreen = ConfigurationManager.AppSettings.EnableFullScreen.GetValueOrDefault(false)
            };

            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.GameServerAddress))
            {
                if (ConfigurationManager.AppSettings.GameServerAddress.Contains(':'))
                {
                    var address = ConfigurationManager.AppSettings.GameServerAddress.Split(':');

                    launchSetting.ServerSetting = new()
                    {
                        IPAddress = address[0],
                        Port = int.Parse(address[1])
                    };
                }
                else launchSetting.ServerSetting = new()
                {
                    IPAddress = ConfigurationManager.AppSettings.GameServerAddress,
                    Port = 25565
                };
            }
        }


        return launchSetting;
    }

    public Dictionary<string, string> CreateDictionary()
        => new Dictionary<string, string>()
        {
            { "GameFolder", GameCore.Root.FullName },
            { "WorkingFolder", GameCore.GetLaunchWorkingFolder(CustomLaunchSetting) },
            { "JavaRuntime", JavaRuntime },
            { "Id", GameCore.Id },
            { "StopMethodId", StopMethodId.ToString() }
        };

    private void OnDownloadProgressChanged(object sender, Windows.Foundation.Collections.ValueSet e)
    {
        var res = MethodResponse.CreateFromValueSet(e);

        if (res.ImplementId == DownloadProgressChangedEventId)
        {
            var jobject = JObject.Parse((string)res.Response);

            SetState(7, new() { { "{message}", jobject["Message"].ToString() } });
        }
    }

    private void OnDownloadCompleted(object sender, Windows.Foundation.Collections.ValueSet e)
    {
        var res = MethodResponse.CreateFromValueSet(e);

        if (res.ImplementId == DownloadCompletedEventId)
        {
            SetState(8);

            DesktopServiceManager.Service.RequestReceived -= OnDownloadCompleted;
            DesktopServiceManager.Service.RequestReceived -= OnDownloadProgressChanged;

            Downloaded = true;
        }
    }

    private void SetState(int state, Dictionary<string, string> keyValuePairs = null)
    {
        var template = ConfigurationManager.AppSettings.CurrentLanguage.GetString($"MinecraftProcessor_State_{state}");

        if (keyValuePairs != null)
            template = template.Replace(keyValuePairs);

        this.StateChanged?.Invoke(this, template);
    }

    private void DesktopService_RequestReceived(object sender, Windows.Foundation.Collections.ValueSet e)
    {
        var res = MethodResponse.CreateFromValueSet(e);

        if (res.ImplementId == ExitedEventId)
        {
            var args = JsonConvert.DeserializeObject<MinecraftExitedArgs>((string)res.Response);

            Exited?.Invoke(this, args);
            DesktopServiceManager.Service.RequestReceived -= DesktopService_RequestReceived;

            SetState(9);
        }

        if (res.ImplementId == ProcessOutputEventId)
        {
            var output = JToken.Parse((string)res.Response).ToString();

            _ = CoreApplication.MainView.Dispatcher.RunAsync(default, () => Outputs.Add(output));
            ProcessOutput?.Invoke(this, output);
        }
    }
}
#endif

#if NETCOREAPP
using Natsurainko.FluentLauncher.Desktop;

public static class LauncherProcess
{
    public static Dictionary<string, object> LaunchMinecraft(Dictionary<string, string> keyValuePairs, LaunchSetting launchSetting)
    {
        launchSetting.JvmSetting.Javaw = new FileInfo(keyValuePairs["JavaRuntime"]);
        launchSetting.WorkingFolder = new DirectoryInfo(keyValuePairs["WorkingFolder"]);

        var locator = new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(keyValuePairs["GameFolder"]);
        var launcher = new MinecraftLauncher(launchSetting, locator);

        try
        {
            if (launchSetting.Account.Type == AccountType.Yggdrasil)
            {
                var account = launchSetting.Account as YggdrasilAccount;
                string base64 = Task.Run(async () => await (await HttpWrapper.HttpGetAsync(account.YggdrasilServerUrl)).Content.ReadAsStringAsync()).GetAwaiter().GetResult().ConvertToBase64();

                var args = Natsurainko.FluentCore.Service.DefaultSettings.DefaultAdvancedArguments.ToList();
                args.Add($"-javaagent:{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries", "authlib-injector-1.1.47.jar").ToPath()}={keyValuePairs["YggdrasilServerUrl"]}");
                args.Add($"-Dauthlibinjector.yggdrasil.prefetched={base64}");

                launchSetting.JvmSetting.AdvancedArguments = args;
            }
        }
        catch { launchSetting.JvmSetting.AdvancedArguments = null; }

        var ExitedEventId = Guid.NewGuid();
        var ProcessOutputEventId = Guid.NewGuid();

        var res = launcher.LaunchMinecraft(keyValuePairs["Id"]);

        void MinecraftExited(object sender, MinecraftExitedArgs e)
        {
            CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
            {
                ImplementId = ExitedEventId,
                Method = "ProgressChanged",
                Response = new
                {
                    RunTime = e.RunTime.Elapsed,
                    e.ExitCode,
                    e.Crashed,
                    e.Outputs
                }
            });

            res.MinecraftExited -= MinecraftExited;
            res.MinecraftProcessOutput -= MinecraftProcessOutput;
            res.Dispose();
        }

        void MinecraftProcessOutput(object sender, IProcessOutput e)
        {
            CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
            {
                ImplementId = ProcessOutputEventId,
                Method = "MinecraftProcessOutput",
                Response = e.GetPrintValue()
            });
        }

        void StopProcess(object sender, ValueSet e)
        {
            var request = MethodRequest.CreateFromValueSet(e);

            if (request.Method == "StopProcess" && request.ImplementId == Guid.Parse(keyValuePairs["StopMethodId"]))
                res.Stop();

            CurrentApplication.DesktopService.RequestReceived -= StopProcess;
        }

        res.MinecraftExited += MinecraftExited;
        res.MinecraftProcessOutput += MinecraftProcessOutput;

        CurrentApplication.DesktopService.RequestReceived += StopProcess;

        var core = locator.GetGameCore(keyValuePairs["Id"]);
        var customSettingFile = new FileInfo(Path.Combine(core.Root.FullName, "versions", core.Id, "fluentlauncher.json"));

        if (!customSettingFile.Exists)
            File.WriteAllText(customSettingFile.FullName, new CustomLaunchSetting().ToJson());
        else
        {
            var customLaunchSetting = JsonConvert.DeserializeObject<CustomLaunchSetting>(File.ReadAllText(customSettingFile.FullName));
            customLaunchSetting.LastLaunchTime = DateTime.Now;

            File.WriteAllText(customSettingFile.FullName, customLaunchSetting.ToJson());
        }

        return new()
        {
            { "Exception", res.Exception },
            { "Arguemnts", res.Arguemnts },
            { "State", res.State },
            { "StartTime", DateTime.Now - res.RunTime?.Elapsed },
            { "ExitedEventId", ExitedEventId },
            { "ProcessOutputEventId", ProcessOutputEventId }
        };
    }

    public static void BeginResourceDownloader(string folder, string id, string progressChangedEventId, string completedEventId)
    {
        var locator = new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(folder);

        var downloader = new ResourceDownloader(locator.GetGameCore(id))
        {
            DownloadProgressChangedAction = (message, progress) =>
            {
                CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
                {
                    ImplementId = Guid.Parse(progressChangedEventId),
                    Method = "ProgressChanged",
                    Response = new
                    {
                        Message = message,
                        Progress = progress
                    }
                });
            }
        };
        ResourceDownloadResponse downloadResponse = null;

        Task.Run(async () => downloadResponse = await downloader.DownloadAsync()).ContinueWith(task =>
        {
            CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
            {
                ImplementId = Guid.Parse(completedEventId),
                Method = "CompletedEvent",
                Response = new
                {
                    downloadResponse?.Total,
                    downloadResponse?.SuccessCount
                }
            });
        });
    }

}
#endif