using Natsurainko.FluentLauncher.Shared.Desktop;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Natsurainko.Toolkits.Text;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Shared.Mapping;

using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentCore.Class.Model.Install.Fabric;
using Natsurainko.FluentCore.Class.Model.Install.Forge;
using Natsurainko.FluentCore.Class.Model.Install.OptiFine;
using Natsurainko.FluentCore.Class.Model.Install.Vanilla;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Installer;

#if WINDOWS_UWP
using Natsurainko.FluentLauncher.Class.Component;

public class DownloaderProcess
{
    public HttpDownloadRequest DownloadRequest { get; private set; }

    public MethodResponse DownloadResponse { get; private set; }

    public event EventHandler<string> StateChanged;

    public event EventHandler<JObject> DownloadProgressChanged;

    public event EventHandler<MethodResponse> DownloadCompleted;

    public readonly Guid DownloadProgressChangedEventId;

    public readonly Guid DownloadCompletedEventId;

    public DownloaderProcess()
    {
        DownloadProgressChangedEventId = Guid.NewGuid();
        DownloadCompletedEventId = Guid.NewGuid();
    }

    public DownloaderProcess(HttpDownloadRequest httpDownloadRequest)
    {
        DownloadRequest = httpDownloadRequest;

        DownloadProgressChangedEventId = Guid.NewGuid();
        DownloadCompletedEventId = Guid.NewGuid();
    }

    public async Task DownloadAsync()
    {
        var downloaderBuilder = MethodRequestBuilder.Create()
            .AddParameter((DownloadRequest, "Natsurainko.Toolkits.Network.Model.HttpDownloadRequest, Natsurainko.Toolkits"))
            .AddParameter(DownloadProgressChangedEventId.ToString())
            .AddParameter(DownloadCompletedEventId.ToString())
            .SetMethod("BeginDownloaderProcess");

        SetState(1);

        await DesktopServiceManager.Service.SendAsyncWithoutResponse(downloaderBuilder.Build());

        DesktopServiceManager.Service.RequestReceived += OnDownloadProgressChanged;
        DesktopServiceManager.Service.RequestReceived += OnDownloadCompleted;

    }

    public async Task DownloadAsync(string methodName, (object, string)[] objects)
    {
        var downloaderBuilder = MethodRequestBuilder.Create();

        foreach ((object, string) obj in objects)
            downloaderBuilder = downloaderBuilder.AddParameter(obj);

        SetState(1);

        await DesktopServiceManager.Service.SendAsyncWithoutResponse(downloaderBuilder.SetMethod(methodName).Build());

        DesktopServiceManager.Service.RequestReceived += OnDownloadProgressChanged;
        DesktopServiceManager.Service.RequestReceived += OnDownloadCompleted;

    }

    public async Task<MethodResponse> WaitForDownloaded()
    {
        while (DownloadResponse == null)
            await Task.Delay(100);

        return DownloadResponse;
    }

    private void OnDownloadProgressChanged(object sender, Windows.Foundation.Collections.ValueSet e)
    {
        var res = MethodResponse.CreateFromValueSet(e);

        if (res.ImplementId == DownloadProgressChangedEventId)
        {
            var jobject = JObject.Parse((string)res.Response);

            SetState(2, new() { { "{message}", jobject["Message"].ToString() } });
            DownloadProgressChanged?.Invoke(this, jobject);
        }
    }

    private void OnDownloadCompleted(object sender, Windows.Foundation.Collections.ValueSet e)
    {
        var res = MethodResponse.CreateFromValueSet(e);

        if (res.ImplementId == DownloadCompletedEventId)
        {
            DownloadResponse = res;

            SetState(3);
            DownloadCompleted?.Invoke(this, DownloadResponse);

            DesktopServiceManager.Service.RequestReceived -= OnDownloadCompleted;
            DesktopServiceManager.Service.RequestReceived -= OnDownloadProgressChanged;
        }

    }

    private void SetState(int state, Dictionary<string, string> keyValuePairs = null)
    {
        var template = ConfigurationManager.AppSettings.CurrentLanguage.GetString($"SimpleDownloader_State_{state}");

        if (keyValuePairs != null)
            template = template.Replace(keyValuePairs);

        this.StateChanged?.Invoke(this, template);
    }

    public void SetState(string state, Dictionary<string, string> keyValuePairs = null)
    {
        var template = state;

        if (keyValuePairs != null)
            template = template.Replace(keyValuePairs);

        this.StateChanged?.Invoke(this, template);
    }
}

#endif

#if NETCOREAPP
using Natsurainko.FluentLauncher.Desktop;

public static class DownloaderProcess
{
    public static void BeginDownloaderProcess(HttpDownloadRequest request, string progressChangedEventId, string completedEventId)
    {
        void ProgressChanged(float progress, string message) => CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
        {
            ImplementId = Guid.Parse(progressChangedEventId),
            Method = "ProgressChangedEvent",
            Response = new
            {
                Message = message,
                Progress = progress
            }
        });

        void Completed(HttpDownloadResponse httpDownloadResponse) => CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
        {
            ImplementId = Guid.Parse(completedEventId),
            Method = "CompletedEvent",
            Response = httpDownloadResponse
        });

        Task.Run(async () => Completed(await HttpWrapper.HttpDownloadAsync(request, ProgressChanged)));
    }

    public static void BeginMinecraftInstaller
        (string type, string folder, string javaPath, object build, string progressChangedEventId, string completedEventId)
    {
        void ProgressChanged(float progress, string message) => CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
        {
            ImplementId = Guid.Parse(progressChangedEventId),
            Method = "ProgressChangedEvent",
            Response = new
            {
                Message = message,
                Progress = progress
            }
        });

        void Completed(InstallerResponse response) => CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
        {
            ImplementId = Guid.Parse(completedEventId),
            Method = "CompletedEvent",
            Response = response
        });

        Task.Run(async () =>
        {
            var locator = new Natsurainko.FluentCore.Module.Launcher.GameCoreLocator(folder);

            IInstaller installer = type switch
            {
                "vanllia" => new MinecraftVanlliaInstaller(locator, (CoreManifestItem)build),
                "forge" => new MinecraftForgeInstaller(locator, (ForgeInstallBuild)build, javaPath),
                "fabric" => new MinecraftFabricInstaller(locator, (FabricInstallBuild)build),
                "optifine" => new MinecraftOptiFineInstaller(locator, (OptiFineInstallBuild)build, javaPath),
                _ => null
            };

            if (installer == null)
                Completed(new InstallerResponse
                {
                    Success = false,
                    GameCore = null,
                    Exception = new ArgumentNullException(nameof(type)),
                });

            installer.ProgressChanged += (object sender, (string, float) e) => ProgressChanged(e.Item2, e.Item1);
            var res = await installer.InstallAsync();

            Completed(res);
        });
    }

}
#endif