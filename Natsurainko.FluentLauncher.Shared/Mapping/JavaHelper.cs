using Natsurainko.FluentLauncher.Shared.Class.Model;
using Natsurainko.FluentLauncher.Shared.Desktop;
using Natsurainko.Toolkits.IO;
using Natsurainko.Toolkits.Network;
using Natsurainko.Toolkits.Network.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Shared.Mapping;

#if WINDOWS_UWP
using Natsurainko.FluentLauncher.Class.Component;
#endif

#if NETCOREAPP
using Natsurainko.FluentLauncher.Desktop;
#endif

public class JavaHelper
{
#if WINDOWS_UWP
    public static async Task<List<string>> SearchJavaRuntime()
    {
        var builder = MethodRequestBuilder.Create()
            .SetMethod("SearchJavaRuntime");

        return (await DesktopServiceManager.Service.SendAsync<List<string>>(builder.Build())).Response;
    }

    public static async Task<JavaInformation> GetJavaInformation(string path)
    {
        var builder = MethodRequestBuilder.Create()
            .AddParameter(path)
            .SetMethod("GetJavaInformation");

        return (await DesktopServiceManager.Service.SendAsync<JavaInformation>(builder.Build())).Response;
    }
#endif

#if NETCOREAPP

    public static List<string> SearchJavaRuntime()
        => Natsurainko.FluentCore.Extension.Windows.Service.JavaHelper.SearchJavaRuntime(new string[] { CurrentApplication.StorageFolder }).ToList();

    public static JavaInformation GetJavaInformation(string path)
    {
        var outputs = new List<string>();
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = $"-jar \"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libraries", "JavaDetails.jar")}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => outputs.Add(e.Data);

        process.Start();
        process.BeginOutputReadLine();

        process.WaitForExit();

        if (outputs.Count == 0)
            return null;

        return JsonConvert.DeserializeObject<JavaInformation>(outputs[0]);
    }

    public static void BeginJavaInstaller(HttpDownloadRequest request, string progressChangedEventId, string completedEventId)
    {
        var runtimefolder = new DirectoryInfo(Path.Combine(CurrentApplication.StorageFolder, "Runtimes"));

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

        string Extract(FileInfo fileInfo)
        {
            if (!runtimefolder.Exists)
                runtimefolder.Create();

            ProgressChanged(-1, "解压中");

            ZipFile.ExtractToDirectory(fileInfo.FullName, runtimefolder.FullName, true);
            DirectoryInfo dir = default;

            using (var zipFile = ZipFile.OpenRead(fileInfo.FullName))
                dir = new DirectoryInfo(Path.Combine(runtimefolder.FullName, zipFile.Entries.First().FullName));

            fileInfo.Delete();

            return dir.Find("javaw.exe").FullName;
        }

        void Completed(bool success, string path) => CurrentApplication.DesktopService.SendResponseAsync(new MethodResponse
        {
            ImplementId = Guid.Parse(completedEventId),
            Method = "CompletedEvent",
            Response = new
            {
                Success = success,
                Path = path
            }
        });

        Task.Run(async () =>
        {
            request.Directory = new DirectoryInfo(Path.Combine(CurrentApplication.StorageFolder, "Downloads"));
            var res = await HttpWrapper.HttpDownloadAsync(request, ProgressChanged);

            if (res.HttpStatusCode != HttpStatusCode.OK)
            {
                Completed(false, null);
                return;
            }

            try
            {
                Completed(true, Extract(res.FileInfo));
            }
            catch
            {
                throw;
            }
        });
    }

#endif
}
