using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using Natsurainko.FluentLauncher.ViewModel.Pages;
using System.Linq;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Natsurainko.FluentLauncher.View.Pages;

public sealed partial class ProcessOutputPage : Page
{
    public ProcessOutputPageVM ViewModel { get; set; }

    public LauncherProcessViewData LauncherProcess { get; set; }

    public AppWindow AppWindow { get; set; }

    public ProcessOutputPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var parmeter = ((AppWindow, LauncherProcessViewData))e.Parameter;

        AppWindow = parmeter.Item1;
        LauncherProcess = parmeter.Item2;

        ViewModel = ViewModelBuilder.Build<ProcessOutputPageVM, Page>(this);
        ViewModel.Outputs = LauncherProcess.Data.Outputs;
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Outputs.CollectionChanged += Outputs_CollectionChanged;
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Outputs.CollectionChanged -= Outputs_CollectionChanged;
    }

    private void Outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // 滚动 1
        ListView.SelectedIndex = ListView.Items.Count - 1;

        try { ListView.ScrollIntoView(ListView.Items.Last()); } catch { }
        try { ListView.ScrollIntoView(ListView.Items.Last()); } catch { }

        /* 滚动 2
        var border = VisualTreeHelper.GetChild(ListView, 0) as Border;
        if (border.Child is ScrollViewer view)
            try
            {
#pragma warning disable CS0618 // 类型或成员已过时
                view.ScrollToVerticalOffset(view.ScrollableHeight);
#pragma warning restore CS0618 // 类型或成员已过时
                view.UpdateLayout();
            }
            catch { }
        */
    }
}
