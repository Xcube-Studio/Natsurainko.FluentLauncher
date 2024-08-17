using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Experimental.GameManagement.Downloader;
using Nrk.FluentCore.Resources;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class DownloadProcessViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private double progress = 0;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string displayState;

    [ObservableProperty]
    private bool isExpanded = true;

    public string ProgressText => Progress.ToString("P1");

}

internal partial class FileDownloadProcessViewModel : DownloadProcessViewModel
{
    [ObservableProperty]
    private FileDownloadProcessState state;

    private readonly CurseForgeClient curseForgeClient = App.GetService<CurseForgeClient>();

    private readonly string _filePath;
    private readonly object _file;

    public FileDownloadProcessViewModel(object file, string filePath)
    {
        _file = file;
        _filePath = filePath;
    }

    public Task Start() => Task.Run(async () =>
    {
        App.DispatcherQueue.SynchronousTryEnqueue(() => State = FileDownloadProcessState.Created);

        string url = default;

        url = _file is CurseForgeFile curseFile
            ? await curseForgeClient.GetFileUrlAsync(curseFile)
            : ((ModrinthFile)_file).Url;

        App.DispatcherQueue.SynchronousTryEnqueue(() =>
        {
            State = FileDownloadProcessState.Downloading;
            Title = Path.GetFileName(_filePath);
        });

        var downloadTask = HttpUtils.Downloader.CreateDownloadTask(
            url,
            _filePath);

        Timer t = new((_) =>
        {
            if (downloadTask.TotalBytes is null)
                return;
            Progress = downloadTask.DownloadedBytes / (double)downloadTask.TotalBytes;
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        var downloadResult = await downloadTask.StartAsync();
        t.Dispose();

        App.DispatcherQueue.SynchronousTryEnqueue(() => State = downloadResult.Type == DownloadResultType.Failed ? FileDownloadProcessState.Faulted : FileDownloadProcessState.Finished);
    });

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(State))
            DisplayState = ResourceUtils.GetValue("Converters", $"_ResourceDownloadProcessState_{State}");
    }

    [RelayCommand]
    public void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{_filePath}"));
    }

    public enum FileDownloadProcessState
    {
        Created = 0,
        Downloading = 1,
        Finished = 2,
        Faulted = 3,
    }
}

internal partial class InstallProcessViewModel : DownloadProcessViewModel
{
    private Action<InstallProcessViewModel> _action;

    public InstallProcessViewModel()
    {
        DisplayState = ResourceUtils.GetValue("Converters", "_CoreInstallProcessState_Created");
    }

    public List<ProgressItem> Progresses { get; } = new();

    public void SetStartAction(Action<InstallProcessViewModel> action) => _action = action;

    public void Start()
    {
        DisplayState = ResourceUtils.GetValue("Converters", "_CoreInstallProcessState_Installing");
        Task.Run(() => _action(this));
    }

    private void UpdateState()
    {
        if (Progresses.Count == Progresses.Where(x => x.IsFinished && !x.IsFaulted).Count())
            DisplayState = ResourceUtils.GetValue("Converters", "_CoreInstallProcessState_Finished");

        Progress = Progresses.Select(x => x.ProgressValue).Sum() / Progresses.Count;
    }

    private void SetFailed() => DisplayState = ResourceUtils.GetValue("Converters", "_CoreInstallProcessState_Faulted");

    internal partial class ProgressItem : ObservableObject
    {
        private ProgressItem _next;

        private readonly Action<ProgressItem> _action;
        private InstallProcessViewModel _installProcess;
        private readonly bool _necessary;

        public ProgressItem(Action<ProgressItem> action, string stepName)
        {
            _action = action;
            StepName = stepName;
        }

        public ProgressItem(Action<ProgressItem> action, string stepName, InstallProcessViewModel installProcess, bool necessary = true)
        {
            _action = action;
            _installProcess = installProcess;
            _necessary = necessary;
            StepName = stepName;
        }

        [ObservableProperty]
        private string stepName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Percentage))]
        private double progressValue = 0;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontForeground))]
        [NotifyPropertyChangedFor(nameof(FontWeight))]
        [NotifyPropertyChangedFor(nameof(FontIconVisibility))]
        [NotifyPropertyChangedFor(nameof(FontIcon))]
        private bool isRunning;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontIcon))]
        private bool isFaulted;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontIcon))]
        private bool isFinished;

        public object FontForeground => IsRunning
                ? App.Current.Resources["ApplicationForegroundThemeBrush"]
                : App.Current.Resources["ApplicationSecondaryForegroundThemeBrush"];

        public FontWeight FontWeight => IsRunning ? FontWeights.SemiBold : FontWeights.Normal;

        public Visibility FontIconVisibility => IsRunning ? Visibility.Collapsed : Visibility.Visible;

        public string FontIcon => IsFinished ? IsFaulted ? "\uE711" : "\uE73E" : IsRunning ? null : "\uE73C";

        public string Percentage => ProgressValue.ToString("P1");

        public void SetNext(ProgressItem progressItem) => _next = progressItem;

        public void SetCoreInstallProcess(InstallProcessViewModel coreInstallProcess) => _installProcess = coreInstallProcess;

        public Task Start()
        {
            App.DispatcherQueue.TryEnqueue(() => IsRunning = true);

            return Task.Run(() => _action(this)).ContinueWith(task =>
            {
                App.DispatcherQueue.TryEnqueue(() =>
                {
                    IsRunning = false;
                    IsFinished = true;
                });

                if (!task.IsFaulted)
                    _next?.Start();
                else App.DispatcherQueue.TryEnqueue(() => IsFaulted = true);
            });
        }

        public void OnProgressChanged(double value) => App.DispatcherQueue.TryEnqueue(() => ProgressValue = value);

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (_necessary && IsFaulted)
                _installProcess.SetFailed();

            _installProcess?.UpdateState();
        }
    }
}