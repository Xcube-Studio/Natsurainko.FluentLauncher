using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Desktop;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.View.Pages.Activities;
using Natsurainko.Toolkits.Network.Model;
using Newtonsoft.Json;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class DownloaderProcessViewData : ViewDataBase<DownloaderProcess>
{
    public DownloaderProcessViewData(DownloaderProcess data) : base(data)
    {
        data.StateChanged += Data_StateChanged;
        data.DownloadProgressChanged += Data_DownloadProgressChanged;
        data.DownloadCompleted += Data_DownloadCompleted;
    }

    [Reactive]
    public string Title { get; set; }

    [Reactive]
    public string State { get; set; }

    [Reactive]
    public string ProgressDescription { get; set; }

    [Reactive]
    public float Progress { get; set; }

    [Reactive]
    public bool IsExpanded { get; set; } = true;

    [Reactive]
    public bool IsIndeterminate { get; set; }

    [Reactive]
    public Visibility OpenFolderVisibility { get; set; } = Visibility.Collapsed;

    [Reactive]
    public string OpenFolderContent { get; set; }

    [Reactive]
    public Visibility OpenLinkVisibility { get; set; } = Visibility.Collapsed;

    [Reactive]
    public string OpenLinkContent { get; set; }

    [Reactive]
    public string ErrorInfo { get; set; }

    public virtual async void BeginDownload()
    {
        await Data.DownloadAsync();

        DispatcherHelper.RunAsync(() =>
        {
            if (!string.IsNullOrEmpty(OpenLinkContent))
                this.OpenLinkVisibility = Visibility.Visible;
        });
    }

    protected virtual void Data_DownloadCompleted(object sender, MethodResponse e) => DispatcherHelper.RunAsync(() =>
    {
        var httpDownloadResponse = JsonConvert.DeserializeObject<HttpDownloadResponse>((string)e.Response, MethodRequestBuilder.JsonConverters);

        this.Progress = 1.0f;
        this.ProgressDescription = httpDownloadResponse.HttpStatusCode == System.Net.HttpStatusCode.OK ? "已完成" : "下载失败";

        if (httpDownloadResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            MainContainer.ShowInfoBarAsync(ErrorInfo, httpDownloadResponse.Message, severity: Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);

        this.OpenFolderContent = Path.Combine(Data.DownloadRequest.Directory.FullName, Data.DownloadRequest.FileName);
        this.IsExpanded = false;
    });

    protected virtual void Data_DownloadProgressChanged(object sender, Newtonsoft.Json.Linq.JObject e) => DispatcherHelper.RunAsync(() =>
    {
        this.Progress = (float)e["Progress"];
        this.ProgressDescription = (string)e["Message"];
    });

    protected virtual void Data_StateChanged(object sender, string e) => DispatcherHelper.RunAsync(() => State = e);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IsIndeterminate))
            this.IsIndeterminate = this.Progress == -1;

        if (e.PropertyName != nameof(OpenFolderVisibility))
            this.OpenFolderVisibility = !string.IsNullOrEmpty(this.OpenFolderContent) ? Visibility.Visible : Visibility.Collapsed;
    }

    public static void CreateDownloadProcess(HttpDownloadRequest httpDownloadRequest, string title)
    {
        /*
        control.IsEnabled = false;

        var launcherProcess = new LauncherProcess(gameCore).CreateViewData<LauncherProcess, LauncherProcessViewData>();

        CacheResources.LauncherProcesses.Insert(0, launcherProcess);

        var hyperlinkButton = new HyperlinkButton { Content = ConfigurationManager.AppSettings.CurrentLanguage.GetString("Info_Launch_L") };
        hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(ActivitiesPage), typeof(ActivityLaunchPage));

        MainContainer.ShowInfoBarAsync(
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Info_Launch_T").Replace("{Id}", gameCore.Id),
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Info_Launch_ST"),
            button: hyperlinkButton);

        await launcherProcess.Launch();

        control.IsEnabled = true;*/
    }

    public static async void CreateModDownloadProcess(CurseForgeModpackViewData curseForgeModpack, Control control)
    {
        control.IsEnabled = false;

        if (string.IsNullOrEmpty(ConfigurationManager.AppSettings.CurrentGameFolder))
        {
            MainContainer.ShowInfoBarAsync(ConfigurationManager.AppSettings.CurrentLanguage.GetString("CP_T1"), severity: Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
            control.IsEnabled = true;

            return;
        }

        var folder = ConfigurationManager.AppSettings.CurrentGameCore != null
            ? await ConfigurationManager.AppSettings.CurrentGameCore.GetStorageModFolder()
            : Path.Combine(ConfigurationManager.AppSettings.CurrentGameFolder, "mods");

        var downloaderProcess = new DownloaderProcess(new HttpDownloadRequest
        {
            Directory = new DirectoryInfo(folder),
            FileName = curseForgeModpack.CurrentFileInfo.FileName,
            Url = curseForgeModpack.CurrentFileInfo.DownloadUrl
        });

        var downloaderProcessViewData = downloaderProcess.CreateViewData<DownloaderProcess, DownloaderProcessViewData>();
        downloaderProcessViewData.Title = ConfigurationManager.AppSettings.CurrentLanguage.GetString("ModDownload_Title").Replace("{fileName}", downloaderProcess.DownloadRequest.FileName);
        downloaderProcessViewData.ErrorInfo = ConfigurationManager.AppSettings.CurrentLanguage.GetString("ModDownload_Failed_T2").Replace("{fileName}", curseForgeModpack.CurrentFileInfo.FileName);
        downloaderProcessViewData.OpenLinkContent = downloaderProcess.DownloadRequest.Url;

        CacheResources.DownloaderProcesses.Insert(0, downloaderProcessViewData);
        downloaderProcessViewData.BeginDownload();

        var hyperlinkButton = new HyperlinkButton { Content = ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_H") };
        hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(ActivitiesPage), typeof(ActivityDownloadPage));

        MainContainer.ShowInfoBarAsync(
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_T").Replace("{title}", downloaderProcess.DownloadRequest.FileName),
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Download_Add_ST"),
            button: hyperlinkButton);

        control.IsEnabled = true;
    }
}
