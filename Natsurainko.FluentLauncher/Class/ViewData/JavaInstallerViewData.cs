using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Desktop;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.View.Pages.Activities;
using Natsurainko.Toolkits.Network.Model;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class JavaInstallerViewData : DownloaderProcessViewData
{
    public JavaInstallerViewData(DownloaderProcess data) : base(data)
    {
    }

    public override async void BeginDownload()
    {
        await Data.DownloadAsync
        (
            "BeginJavaInstaller",
            new (object, string)[]
            {
                (Data.DownloadRequest, "Natsurainko.Toolkits.Network.Model.HttpDownloadRequest, Natsurainko.Toolkits"),
                (Data.DownloadProgressChangedEventId.ToString(), string.Empty.GetType().FullName),
                (Data.DownloadCompletedEventId.ToString(), string.Empty.GetType().FullName)
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
        var keyValuePairs = JObject.Parse((string)e.Response);
        bool success = (bool)keyValuePairs["Success"];

        this.Progress = 1.0f;
        this.ProgressDescription = success ? "已完成" : "安装失败";

        if (!success)
            MainContainer.ShowInfoBarAsync($"安装 Java 运行时失败：", string.Empty, severity: Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        else this.OpenFolderContent = keyValuePairs["Path"].ToString();

        this.IsExpanded = false;
    });

    protected override void Data_DownloadProgressChanged(object sender, JObject e)
    {
        base.Data_DownloadProgressChanged(sender, e);

        if ((float)e["Progress"] == -1)
            Data.SetState((string)e["Message"]);
    }

    public static void CreateJavaInstallProcess(string url, string javaName, Control control)
    {
        control.IsEnabled = false;

        var downloaderProcess = new DownloaderProcess(new HttpDownloadRequest { Url = url });

        var javaInstallerViewData = downloaderProcess.CreateViewData<DownloaderProcess, JavaInstallerViewData>();
        javaInstallerViewData.Title = ConfigurationManager.AppSettings.CurrentLanguage.GetString("JavaInstaller_Title").Replace("{title}", javaName);
        javaInstallerViewData.OpenLinkContent = url;

        CacheResources.DownloaderProcesses.Insert(0, javaInstallerViewData);
        javaInstallerViewData.BeginDownload();

        var hyperlinkButton = new HyperlinkButton { Content = ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_H") };
        hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(ActivitiesPage), typeof(ActivityDownloadPage));

        MainContainer.ShowInfoBarAsync(
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_T").Replace("{title}", javaInstallerViewData.Title),
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_ST"),
            button: hyperlinkButton);

        control.IsEnabled = true;
    }
}
