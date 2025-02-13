using Windows.Foundation;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class DefaultPage : Page, IBreadcrumbBarAware
{
    DefaultViewModel VM => (DefaultViewModel)DataContext;

    string IBreadcrumbBarAware.Route => "Cores";

    public DefaultPage()
    {
        InitializeComponent();
    }

    private void WrapPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var transform = scrollViewer.TransformToVisual(App.MainWindow.Content);
        var absolutePosition = transform.TransformPoint(new Point(0, 0));

        ThirdRowDefinition.Height = new GridLength(absolutePosition.Y);
    }
}
