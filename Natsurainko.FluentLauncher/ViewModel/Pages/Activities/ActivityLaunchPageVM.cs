using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.ViewData;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Activities;

public class ActivityLaunchPageVM : ViewModelBase<Page>
{
    public ActivityLaunchPageVM(Page control) : base(control)
    {
        LauncherProcesses = new(CacheResources.LauncherProcesses);
        TipsVisibility = LauncherProcesses != null && LauncherProcesses.Any() ? Visibility.Collapsed : Visibility.Visible;
        LauncherProcesses.CollectionChanged += LauncherProcesses_CollectionChanged;

        control.Unloaded += Control_Unloaded;
    }

    [Reactive]
    public Visibility TipsVisibility { get; set; }

    [Reactive]
    public ObservableCollection<LauncherProcessViewData> LauncherProcesses { get; set; }

    private void Control_Unloaded(object sender, RoutedEventArgs e)
    {
        LauncherProcesses.CollectionChanged -= LauncherProcesses_CollectionChanged;
        Control.Unloaded -= Control_Unloaded;
    }

    private void LauncherProcesses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        => TipsVisibility = LauncherProcesses != null && LauncherProcesses.Any() ? Visibility.Collapsed : Visibility.Visible;
}
