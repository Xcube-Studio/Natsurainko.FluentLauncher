using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Home;
using Windows.Foundation;

namespace Natsurainko.FluentLauncher.Views.Home;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        LaunchButton.Focus(FocusState.Programmatic);
    }

    private void Flyout_Opened(object sender, object e)
    {
        var vm = (HomeViewModel)DataContext;
        listView.ScrollIntoView(vm.ActiveGameInfo);
    }

    private void DropDownButton_Click(object sender, RoutedEventArgs e)
    {
        var transform = DropDownButton.TransformToVisual(Grid);
        var absolutePosition = transform.TransformPoint(new Point(0, 0));

        listView.MaxHeight = absolutePosition.Y - 50;

        if (this.ActualWidth > 550)
        {
            listView.MaxWidth = 400;
            listView.Width = double.NaN;
        }
        else
        {
            listView.MaxWidth = 430;
            listView.Width = 430;
        }
    }
}
