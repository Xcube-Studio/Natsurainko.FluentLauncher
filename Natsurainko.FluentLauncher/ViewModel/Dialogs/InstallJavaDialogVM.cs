using Natsurainko.FluentLauncher.Class.AppData;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Dialogs;

public class InstallJavaDialogVM : ViewModelBase<ContentDialog>
{
    public InstallJavaDialogVM(ContentDialog control) : base(control)
    {
        DownloadSources = GlobalResources.OpenJdkDownloadSources;
    }

    [Reactive]
    public Dictionary<string, KeyValuePair<string, string>[]> DownloadSources { get; set; }

    [Reactive]
    public string CurrentDownloadSource { get; set; }

    [Reactive]
    public KeyValuePair<string, string>[] Urls { get; set; }

    [Reactive]
    public KeyValuePair<string, string>? CurrentUrl { get; set; }

    [Reactive]
    public bool ConfirmEnabled { get; set; }

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentDownloadSource) && DownloadSources.ContainsKey(CurrentDownloadSource))
            Urls = DownloadSources[CurrentDownloadSource];

        ConfirmEnabled = CurrentUrl != null;
    }
}
