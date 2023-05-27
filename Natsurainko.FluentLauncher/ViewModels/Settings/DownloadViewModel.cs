using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Components.Mvvm;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

public partial class DownloadViewMoel : SettingViewModel
{
    public DownloadViewMoel() : base() { }
}

public partial class DownloadViewMoel
{
    [ObservableProperty]
    private string currentDownloadSource;

    [ObservableProperty]
    private int maxDownloadThreads;

    [ObservableProperty]
    private bool enableFragmentDownload;
}