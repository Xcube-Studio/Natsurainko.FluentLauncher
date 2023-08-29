using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Classes.Enums;
using Natsurainko.FluentLauncher.Services.Storage;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Xaml;
using Nrk.FluentCore.Classes.Datas.Download;
using Nrk.FluentCore.Utils;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Classes.Data.UI;

internal partial class DownloadProcess : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    private double progress;

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

        if (e.PropertyName ==  nameof(State))
            DisplayState = ResourceUtils.GetValue("Converters", $"_ResourceDownloadProcessState_{State}");
    }

    [RelayCommand]
    public void OpenFolder()
    {
        using var process = Process.Start(new ProcessStartInfo("explorer.exe", $"/select,{_filePath}"));
    }
}
