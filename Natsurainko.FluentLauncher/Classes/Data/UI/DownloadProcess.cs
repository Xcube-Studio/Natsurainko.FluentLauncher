using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Classes.Enums;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Text;

namespace Natsurainko.FluentLauncher.Classes.Data.UI;

internal partial class DownloadProcess : ObservableObject
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

internal partial class ResourceDownloadProcess : DownloadProcess
{
    [ObservableProperty]
    private ResourceDownloadProcessState state;

    private readonly string _filePath;
    private readonly object _file;

    public ResourceDownloadProcess(object file, string filePath)
    {
        _file = file;
        _filePath = filePath;
    }

    public Task Start() => Task.Run(async () =>
    {
        App.DispatcherQueue.SynchronousTryEnqueue(() => State = ResourceDownloadProcessState.Created);

        string url = default;

        url = _file is CurseFile curseFile
            ? App.GetService<InterfaceCacheService>().CurseForgeClient.GetCurseFileDownloadUrl(curseFile)
            : ((ModrinthFile)_file).Url;

        App.DispatcherQueue.SynchronousTryEnqueue(() =>
        {
            State = ResourceDownloadProcessState.Downloading;
            Title = Path.GetFileName(_filePath);
        });

        var downloadResult = await HttpUtils.DownloadElementAsync(new DownloadElement
        {
            AbsolutePath = _filePath,
            Url = url
        },
        downloadSetting: new DownloadSetting
        {
            EnableLargeFileMultiPartDownload = false
        },
        perSecondProgressChangedAction: pro => App.DispatcherQueue.TryEnqueue(() => Progress = pro));

        App.DispatcherQueue.SynchronousTryEnqueue(() => State = downloadResult.IsFaulted ? ResourceDownloadProcessState.Faulted : ResourceDownloadProcessState.Finished);
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
}

internal partial class CoreInstallProcess : DownloadProcess
{
    public List<ProgressItem> Progresses { get; } = new();

    private Action<CoreInstallProcess> _action;

    public CoreInstallProcess()
    {
        DisplayState = ResourceUtils.GetValue("Converters", "_CoreInstallProcessState_Created");
    }

    public void SetStartAction(Action<CoreInstallProcess> action) => _action = action;

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

        private readonly Action<ProgressItem> _action;
        private CoreInstallProcess _installProcess;
        private readonly bool _necessary;

        private ProgressItem _next;

        public ProgressItem(Action<ProgressItem> action, string stepName)
        {
            _action = action;

            StepName = stepName;
        }

        public ProgressItem(Action<ProgressItem> action, string stepName, CoreInstallProcess installProcess, bool necessary = true)
        {
            _action = action;
            _installProcess = installProcess;
            _necessary = necessary;

            StepName = stepName;
        }

        public void SetNext(ProgressItem progressItem) => _next = progressItem;

        public void SetCoreInstallProcess(CoreInstallProcess coreInstallProcess) => _installProcess = coreInstallProcess;

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