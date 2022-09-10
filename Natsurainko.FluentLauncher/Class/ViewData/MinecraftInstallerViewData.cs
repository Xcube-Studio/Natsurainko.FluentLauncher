using Natsurainko.FluentCore.Class.Model.Install;
using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Desktop;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.View.Pages.Activities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class MinecraftInstallerViewData : DownloaderProcessViewData
{
    public ModLoaderType? ModLoaderType { get; set; }

    public object InstallBuild { get; set; }

    public MinecraftInstallerViewData(DownloaderProcess data) : base(data)
    {
    }

    public override async void BeginDownload()
    {
        Data.SetState("准备开始安装");
        var strType = string.Empty.GetType().FullName;

        await Data.DownloadAsync
        (
            "BeginMinecraftInstaller",
            new (object, string)[]
            {
                (ModLoaderType.HasValue ? ModLoaderType.Value.ToString().ToLower() : "vanllia", strType),
                (ConfigurationManager.AppSettings.CurrentGameFolder, strType),
                (ConfigurationManager.AppSettings.CurrentJavaRuntime, strType),
                (InstallBuild, $"{InstallBuild.GetType().FullName}, Natsurainko.FluentCore"),
                (Data.DownloadProgressChangedEventId.ToString(), strType),
                (Data.DownloadCompletedEventId.ToString(), strType)
            }
        );

        DispatcherHelper.RunAsync(() =>
        {
            if (!string.IsNullOrEmpty(OpenLinkContent))
                this.OpenLinkVisibility = Visibility.Visible;
        });
    }

    protected override void Data_DownloadCompleted(object sender, MethodResponse e) => DispatcherHelper.RunAsync(() =>
    {
        var installerResponse = JsonConvert.DeserializeObject<InstallerResponse>((string)e.Response, MethodRequestBuilder.JsonConverters);

        this.Progress = 1.0f;
        this.ProgressDescription = installerResponse.Success ? "已完成" : "安装失败";

        if (!installerResponse.Success)
            MainContainer.ShowInfoBarAsync($"安装新的游戏核心 {this.Title} 失败：", string.Empty, severity: Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);

        this.IsExpanded = false;
    });

    protected override void Data_DownloadProgressChanged(object sender, JObject e)
    {
        base.Data_DownloadProgressChanged(sender, e);

        if ((float)e["Progress"] == -1)
            Data.SetState((string)e["Message"]);
    }

    protected override void Data_StateChanged(object sender, string e)
        => DispatcherHelper.RunAsync(() => State = e.Replace("下载中", "安装中"));

    public static void CreateMinecraftInstallProcess(object build, string title, Control control, ModLoaderType? modLoaderType = null)
    {
        control.IsEnabled = false;

        var downloaderProcess = new DownloaderProcess();

        var minecraftInstaller = downloaderProcess.CreateViewData<DownloaderProcess, MinecraftInstallerViewData>();
        minecraftInstaller.Title = $"安装新的游戏核心：{title}";
        minecraftInstaller.ModLoaderType = modLoaderType;
        minecraftInstaller.InstallBuild = build;

        CacheResources.DownloaderProcesses.Insert(0, minecraftInstaller);
        minecraftInstaller.BeginDownload();

        var hyperlinkButton = new HyperlinkButton { Content = ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_H") };
        hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(ActivitiesPage), typeof(ActivityDownloadPage));

        MainContainer.ShowInfoBarAsync(
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_T").Replace("{title}", minecraftInstaller.Title),
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_ST"),
            button: hyperlinkButton);

        control.IsEnabled = true;
    }
}
