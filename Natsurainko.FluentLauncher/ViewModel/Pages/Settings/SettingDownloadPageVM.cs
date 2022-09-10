using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Mapping;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Settings;

public class SettingDownloadPageVM : ViewModelBase<Page>
{
    public SettingDownloadPageVM(Page control) : base(control)
    {
        DownloadSources = GlobalResources.DownloadSources;

        CurrentDownloadSource = string.IsNullOrEmpty(ConfigurationManager.AppSettings.CurrentDownloadSource) ? AppSettings.Default.CurrentDownloadSource : ConfigurationManager.AppSettings.CurrentDownloadSource;

        MaxDownloadThreads = ConfigurationManager.AppSettings.MaxDownloadThreads ?? (int)AppSettings.Default.MaxDownloadThreads;
        EnableFragmentDownload = ConfigurationManager.AppSettings.EnableFragmentDownload ?? (bool)AppSettings.Default.EnableFragmentDownload;
    }

    [Reactive]
    public List<string> DownloadSources { get; set; }

    [Reactive]
    public string CurrentDownloadSource { get; set; }

    [Reactive]
    public int MaxDownloadThreads { get; set; }

    [Reactive]
    public bool EnableFragmentDownload { get; set; }

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        DispatcherHelper.RunAsync(() =>
        {
            ConfigurationManager.AppSettings.CurrentDownloadSource = CurrentDownloadSource;
            ConfigurationManager.AppSettings.MaxDownloadThreads = MaxDownloadThreads;

            ConfigurationManager.AppSettings.EnableFragmentDownload = EnableFragmentDownload;
            ConfigurationManager.Configuration.Save();
        });

        if (e.PropertyName == nameof(CurrentDownloadSource))
            DefaultSettings.SetDownloadSource(CurrentDownloadSource);
    }
}
