using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Utils;
using Nrk.FluentCore.GameManagement.Downloader;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

#if FLUENT_LAUNCHER_PREVIEW_CHANNEL

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class UpdateDialogViewModel : ObservableObject, IDialogParameterAware
{
    private JsonNode _releaseJson = null!;
    private ContentDialog _dialog;

    private readonly UpdateService _updateService;

    public UpdateDialogViewModel(UpdateService updateService)
    {
        _updateService = updateService;
    }

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
    [NotifyPropertyChangedFor(nameof(CanCancel))]
    [NotifyCanExecuteChangedFor(nameof(UpdateCommand))]
    public partial bool Running { get; set; }

    public Visibility ProrgessVisibility => Running ? Visibility.Visible : Visibility.Collapsed;

    public string ProgressText => Progress.ToString("P0");

    public bool CanCancel => !Running;

    void IDialogParameterAware.HandleParameter(object param)
    {
        _releaseJson = (JsonNode)param;

        TagName = _releaseJson["tag_name"]!.GetValue<string>();
        Body = _releaseJson["body"]!.GetValue<string>();
        PublishedAt = _releaseJson["published_at"]!.GetValue<string>();
    }

    [RelayCommand]
    void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _dialog = grid.FindName("Dialog") as ContentDialog;
    }

    [RelayCommand]
    async Task Update()
    {
        Running = true;

        #region Check for installer update
        ActionName = "Check Package Installer Update";

        var (installerHasUpate, installerDownloadUrl) = await _updateService.CheckInstallerUpdateRelease();

        if (installerHasUpate)
        {
            ActionName = "Downloading Package Installer";

            // Download installer
            var downloadTask = _updateService.CreatePackageInstallerDownloadTask(installerDownloadUrl!);

            downloadTask.BytesDownloaded += (size) =>
            {
                double progress = downloadTask.TotalBytes is null ? 0 : downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes;
                App.DispatcherQueue.TryEnqueue(() => Progress = progress);
            };

            var result = await downloadTask.StartAsync();

            if (result.Type == Nrk.FluentCore.GameManagement.Downloader.DownloadResultType.Failed)
            {
                // Show error dialog
                return;
            }
        }

        #endregion

        #region Download update package

        ActionName = "Downloading Update Package";

        var packageDownloadTask = _updateService.CreateUpdatePackageDownloadTask(_releaseJson);
        packageDownloadTask.BytesDownloaded += (size) =>
        {
            double progress = packageDownloadTask.TotalBytes is null ? 0 : packageDownloadTask.DownloadedBytes / (double)packageDownloadTask.TotalBytes;
            App.DispatcherQueue.TryEnqueue(() => Progress = progress);
        };

        var packageResult = await packageDownloadTask.StartAsync();

        if (packageResult.Type == Nrk.FluentCore.GameManagement.Downloader.DownloadResultType.Failed)
        {
            // Show error dialog
            return;
        }

        #endregion

        #region Install update

        ActionName = "Running Package Installer";
        IsIndeterminate = true;

        var (success, error) = await _updateService.RunInstaller();
        if (!success)
        {
            Running = false;
            _dialog.Hide();
        }

        #endregion
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    void Cancel() => _dialog.Hide();
}

#endif