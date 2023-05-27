using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CurseForgePage : Page
{
    public CurseForgePage()
    {
        this.InitializeComponent();
    }

    private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (e.NewSize.Width < 800)
        {
            SplitView.DisplayMode = SplitViewDisplayMode.Overlay;
            SplitViewGrid.Margin = new Thickness(0, 0, 0, 0);
        }
        else
        {
            SplitView.DisplayMode = SplitViewDisplayMode.Inline;
            SplitViewGrid.Margin = new Thickness(0, 0, 10, 0);
        }
    }
}
