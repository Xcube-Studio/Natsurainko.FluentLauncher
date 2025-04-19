using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Network;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class UpdateDialogViewModel(UpdateService updateService) : DialogVM
{
    private JsonNode _releaseJson = null!;

    [ObservableProperty]
    public partial string TagName { get; set; }

    [ObservableProperty]
    public partial string Body { get; set; }

    [ObservableProperty]
    public partial string PublishedAt { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    public partial double Progress { get; set; }

    [ObservableProperty]
    public partial bool IsIndeterminate { get; set; } = false;

    [ObservableProperty]
    public partial string ActionName { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProrgessVisibility))]
    [NotifyPropertyChangedFor(nameof(Enable))]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    public partial bool Running { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProxyBoxVisibility))] 
    public partial bool UseProxy { get; set; }

    [ObservableProperty]
    public partial string ProxyUrl { get; set; } = "https://source.cubestructor.cc";

    [ObservableProperty]
    public partial Visibility ErrorTipVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial string ErrorLogPath { get; set; }

    public Visibility ProxyBoxVisibility => UseProxy ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ProrgessVisibility => Running ? Visibility.Visible : Visibility.Collapsed;

    public string ProgressText => Progress.ToString("P0");

    public bool Enable => !Running;

    public override void HandleParameter(object param)
    {
        _releaseJson = (JsonNode)param;

        TagName = _releaseJson["tag_name"]!.GetValue<string>();
        Body = _releaseJson["body"]!.GetValue<string>();
        PublishedAt = _releaseJson["published_at"]!.GetValue<string>();
    }

    [RelayCommand]
    async Task Update()
    {
        IsIndeterminate = false;
        ErrorTipVisibility = Visibility.Collapsed;
        Running = true;

        #region Check for installer update
        ActionName = "Check Package Installer Update";
        ProxyUrl = ProxyUrl.TrimEnd("//".ToCharArray()).TrimEnd('/') + "/";

        var (installerHasUpate, installerDownloadUrl) = await updateService.CheckInstallerUpdateRelease();

        if (installerHasUpate)
        {
            ActionName = "Downloading Package Installer";

            var downloadTask = updateService.CreatePackageInstallerDownloadTask(installerDownloadUrl!, UseProxy ? ProxyUrl : null);
            using (System.Timers.Timer timer = new(500))
            {
                timer.Elapsed += (sender, e) => App.DispatcherQueue.TryEnqueue(() => 
                    Progress = downloadTask.TotalBytes is null ? 0 : downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
                timer.Start();

                var result = await downloadTask.StartAsync();
                timer.Stop();
                Progress = downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes;

                if (result.Type == Nrk.FluentCore.GameManagement.Downloader.DownloadResultType.Failed)
                {
                    // Show error dialog
                    return;
                }
            }
        }

        #endregion

        #region Download update package
        ActionName = "Downloading Update Package";

        var packageDownloadTask = updateService.CreateUpdatePackageDownloadTask(_releaseJson, UseProxy ? ProxyUrl : null);
        using (System.Timers.Timer timer = new(500))
        {
            timer.Elapsed += (sender, e) => App.DispatcherQueue.TryEnqueue(() =>
                Progress = packageDownloadTask.TotalBytes is null ? 0 : packageDownloadTask.DownloadedBytes / (double)packageDownloadTask.TotalBytes);
            timer.Start();

            var packageResult = await packageDownloadTask.StartAsync();
            timer.Stop();
            Progress = packageDownloadTask.DownloadedBytes / (double)packageDownloadTask.TotalBytes;

            if (packageResult.Type == Nrk.FluentCore.GameManagement.Downloader.DownloadResultType.Failed)
            {
                // Show error dialog
                return;
            }
        }

        #endregion

        #region Install update

        ActionName = "Running Package Installer";
        IsIndeterminate = true;

        var (success, logFile) = await updateService.RunInstaller();

        if (!success)
        {
            Running = false;
            ErrorLogPath = logFile;
            ErrorTipVisibility = Visibility.Visible;
        }

        #endregion
    }

    [RelayCommand]
    void Cancel() => Dialog.Hide();

    [RelayCommand]
    void ShowErrorLog()
    {
        using var _ = Process.Start("notepad.exe", ErrorLogPath);
    }
}