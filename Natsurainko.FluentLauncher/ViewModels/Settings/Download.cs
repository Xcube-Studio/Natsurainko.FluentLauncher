using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Components.Mvvm;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

public partial class Download : SettingViewModel
{
    public Download() : base() { }
}

public partial class Download
{
    [ObservableProperty]
    private string currentDownloadSource;

    [ObservableProperty]
    private int maxDownloadThreads;

    [ObservableProperty]
    private bool enableFragmentDownload;
}