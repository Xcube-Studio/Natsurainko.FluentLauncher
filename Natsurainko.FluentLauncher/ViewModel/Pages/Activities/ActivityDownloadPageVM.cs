using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.ViewData;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Activities;

public class ActivityDownloadPageVM : ViewModelBase<Page>
{
    public ActivityDownloadPageVM(Page control) : base(control)
    {
        DownloaderProcesses = new(CacheResources.DownloaderProcesses);
        TipsVisibility = DownloaderProcesses != null && DownloaderProcesses.Any() ? Visibility.Collapsed : Visibility.Visible;
        DownloaderProcesses.CollectionChanged += DownloaderProcesses_CollectionChanged;

        control.Unloaded += Control_Unloaded;
    }

    [Reactive]
    public Visibility TipsVisibility { get; set; }

    [Reactive]
    public ObservableCollection<DownloaderProcessViewData> DownloaderProcesses { get; set; }

    private void Control_Unloaded(object sender, RoutedEventArgs e)
    {
        DownloaderProcesses.CollectionChanged -= DownloaderProcesses_CollectionChanged;
        Control.Unloaded -= Control_Unloaded;
    }

    private void DownloaderProcesses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        => TipsVisibility = DownloaderProcesses != null && DownloaderProcesses.Any() ? Visibility.Collapsed : Visibility.Visible;
}
