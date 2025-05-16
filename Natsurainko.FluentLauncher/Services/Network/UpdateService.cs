#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Downloader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.Network;

internal partial class UpdateService(
    DownloadService downloadService, 
    LocalStorageService localStorageService, 
    INotificationService notificationService,
    HttpClient httpClient)
{
#if ENABLE_LOAD_EXTENSIONS
    public bool ENABLE_LOAD_EXTENSIONS = true;
#else
    public bool ENABLE_LOAD_EXTENSIONS = false;
#endif

    public async Task<(bool, ReleaseModel?)> CheckLauncherUpdateInformation()
    {
        const string githubApi = "https://api.github.com/repos/Xcube-Studio/Natsurainko.FluentLauncher/releases";

        string releasesContent = await httpClient.GetStringAsync(githubApi);
        string pattern = @"(?<=``` json)([\s\S]+?)(?=```)";
        ReleaseModel[] releases = [.. JsonSerializer.Deserialize(releasesContent, SerializerContext.Default.ReleaseModelArray)!
            .Where(releaseModel => releaseModel.TagName.Contains("pre-release") && releaseModel.IsPreRelease)
            .OrderByDescending(releaseModel => DateTime.Parse(releaseModel.PublishedAt))];

        foreach (var releaseModel in releases)
        {
            Match match = Regex.Match(releaseModel.Body, pattern);

            if (!match.Success)
                continue;

            try
            {
                PublishModel publishModel = JsonSerializer.Deserialize(match.Groups[1].Value, SerializerContext.Default.PublishModel)
                    ?? throw new InvalidDataException();

                if (Version.Parse(publishModel.CurrentPreviewVersion) > Version.Parse(App.Version.GetVersionString()) && 
                    ENABLE_LOAD_EXTENSIONS == publishModel.EnableLoadExtensions)
                    return (true, releaseModel);
            }
            catch { }
        }

        return (false, null);
    }

    public async Task<(bool, AssetModel?)> CheckInstallerUpdateInfomation()
    {
        const string githubApi = "https://api.github.com/repos/Xcube-Studio/FluentLauncher.Preview.Installer/releases/latest";

        string releasesContent = await httpClient.GetStringAsync(githubApi);
        ReleaseModel releases = JsonSerializer.Deserialize(releasesContent, SerializerContext.Default.ReleaseModel)
            ?? throw new InvalidDataException();

        AssetModel? asset = releases.Assets.FirstOrDefault(a => a.Name == $"FluentLauncher.CommandLineInstaller-win-{Arch}.exe");
        FileInfo file = localStorageService.GetFile($"launcher-update\\FluentLauncher.CommandLineInstaller-win-{Arch}.exe");

        if (!file.Exists || file.Length != asset?.Size) return (true, asset);

        return (false, null);
    }

    public DownloadTask DownloadAsset(AssetModel assetModel, string? proxy = null)
    {
        string url = assetModel.DownloadUrl;
        string file = $"launcher-update\\{assetModel.Name}";

        if (!string.IsNullOrEmpty(proxy))
            url = proxy + url;

        return downloadService.Downloader.CreateDownloadTask(url, localStorageService.GetFile(file).FullName);
    }

    public async Task<(bool, string?)> RunInstaller(FileInfo installerFile)
    {
        string logFileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log";

        if (!installerFile.Exists) 
            throw new FileNotFoundException("Installer not found");

        using var process = Process.Start(new ProcessStartInfo()
        {
            FileName = installerFile.FullName,
            UseShellExecute = true,
            Verb = "runas",
            WorkingDirectory = installerFile.DirectoryName,
            Arguments = $"--logFilePath {logFileName}",
            CreateNoWindow = true
        }) ?? throw new InvalidOperationException();

        await process.WaitForExitAsync();

        // If the installer exited without error, the launcher should be closed during the installation.
        return (false, localStorageService.GetFile($"launcher-update\\{logFileName}").FullName);
    }

    private static string Arch { get; } = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.Arm64 => "arm64",
        _ => throw new NotSupportedException("not supported architecture")
    };

    public void CheckLaunchUpdateAfterApplicationStarted(IDialogActivationService<ContentDialogResult> dialogs) => Task.Run(async () =>
    {
        var (hasUpdate, release) = await CheckLauncherUpdateInformation();

        if (hasUpdate)
        {
            notificationService.Show(new ActionNotification
            {
                Title = LocalizedStrings.Notifications__LauncherHasUpdate,
                Message = release!.TagName,
                Type = NotificationType.Warning,
                Delay = 20,
                GetActionButton = () => new HyperlinkButton()
                {
                    Command = new AsyncRelayCommand(async () => await dialogs.ShowAsync("UpdateDialog", release!)),
                    Content = LocalizedStrings.Dialogs_UpdateDialog_Button_Text
                }
            });
        }
    });
}


internal class AssetModel
{
    [JsonPropertyName("name")]
    public required string Name { set; get; }

    [JsonPropertyName("browser_download_url")]
    public required string DownloadUrl { set; get; }

    [JsonPropertyName("size")]
    public required long Size { set; get; }
}

internal class ReleaseModel
{
    [JsonPropertyName("tag_name")]
    public required string TagName { get; set; }

    [JsonPropertyName("published_at")]
    public required string PublishedAt { set; get; }

    [JsonPropertyName("prerelease")]
    public required bool IsPreRelease { get; set; }

    [JsonPropertyName("body")]
    public required string Body { set; get; }

    [JsonPropertyName("assets")]
    public required AssetModel[] Assets { get; set; }

    [JsonPropertyName("html_url")]
    public required string Url { set; get; }
}

internal class PublishModel
{
    [JsonPropertyName("commit")]
    public required string Commit { get; set; }

    [JsonPropertyName("build")]
    public int Build { get; set; }

    [JsonPropertyName("releaseTime")]
    public DateTime ReleaseTime { get; set; }

    [JsonPropertyName("currentPreviewVersion")]
    public required string CurrentPreviewVersion { get; set; }

    [JsonPropertyName("previousStableVersion")]
    public required string PreviousPreviewVersion { get; set; }

    [JsonPropertyName("enableLoadExtensions")]
    public bool EnableLoadExtensions { get; set; }
}

[JsonSerializable(typeof(AssetModel))]
[JsonSerializable(typeof(AssetModel[]))]
[JsonSerializable(typeof(ReleaseModel))]
[JsonSerializable(typeof(ReleaseModel[]))]
[JsonSerializable(typeof(PublishModel))]
internal partial class SerializerContext : JsonSerializerContext { }
#endif