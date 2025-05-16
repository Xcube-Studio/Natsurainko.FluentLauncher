#if FLUENT_LAUNCHER_PREVIEW_CHANNEL
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Storage;
using Nrk.FluentCore.GameManagement.Downloader;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

internal partial class UpdateDialogViewModel(
    UpdateService updateService,
    LocalStorageService localStorageService) : DialogVM
{
    [ObservableProperty]
    public partial ReleaseModel ReleaseModel { get; private set; }

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
    public partial bool UseProxy { get; set; } = true;

    [ObservableProperty]
    public partial Visibility ErrorTipVisibility { get; set; } = Visibility.Collapsed;

    [ObservableProperty]
    public partial string ErrorLogPath { get; set; }

    public Visibility ProrgessVisibility => Running ? Visibility.Visible : Visibility.Collapsed;

    public string ProgressText => Progress.ToString("P0");

    public bool Enable => !Running;

    private static string Arch { get; } = RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => "x64",
        Architecture.Arm64 => "arm64",
        _ => throw new NotSupportedException("not supported architecture")
    };

    public override void HandleParameter(object param) => ReleaseModel = param as ReleaseModel;

    [RelayCommand]
    async Task Update()
    {
        IsIndeterminate = false;
        ErrorTipVisibility = Visibility.Collapsed;
        Running = true;

        bool success = false;
        string logPath = null;

        try
        {
            #region Check for installer update

            ActionName = "Check Package Installer Update";

            var (installerHasUpate, installerAssest) = await updateService.CheckInstallerUpdateInfomation();
            FileInfo installerFile;

            if (installerHasUpate)
            {
                ActionName = "Downloading Package Installer";
                installerFile = await Download(installerAssest);
            }
            else installerFile = localStorageService.GetFile($"launcher-update\\FluentLauncher.CommandLineInstaller-win-{Arch}.exe");

            #endregion

            #region Download update package

            ActionName = "Downloading Update Package";
            FileInfo packageFile = await Download(ReleaseModel.Assets.First(a => a.Name == $"updatePackage-{Arch}.zip"));

            #endregion

            #region Install update

            ActionName = "Running Package Installer";
            IsIndeterminate = true;

            (success, logPath) = await updateService.RunInstaller(installerFile);

            #endregion
        }
        catch (Exception ex)
        {

        }

        if (!success)
        {
            Running = false;
            ErrorLogPath = logPath;
            ErrorTipVisibility = Visibility.Visible;
        }
    }

    [RelayCommand]
    void Cancel() => Dialog.Hide();

    [RelayCommand]
    void ShowErrorLog()
    {
        using var _ = Process.Start("notepad.exe", ErrorLogPath);
    }

    private async Task<FileInfo> Download(AssetModel asset)
    {
        string proxy = UseProxy ? "https://source.cubestructor.cc/" : string.Empty;
        DownloadTask downloadTask = updateService.DownloadAsset(asset, proxy);

        using System.Timers.Timer timer = new(500);
        timer.Elapsed += (sender, e) =>
        {
            Dispatcher.TryEnqueue(() =>
            {
                Progress = downloadTask.TotalBytes is null ? 0 : downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes;
            });
        };
            
        timer.Start();

        try
        {
            var result = await downloadTask.StartAsync();

            if (result.Type == DownloadResultType.Failed)
                throw result.Exception;
        }
        finally
        {
            timer.Stop();
            await Dispatcher.EnqueueAsync(() => Progress = downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes);
        }

        return localStorageService.GetFile($"launcher-update\\{asset.Name}");
    }
}

#endif