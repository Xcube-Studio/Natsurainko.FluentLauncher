using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads;

namespace Natsurainko.FluentLauncher.Views.Downloads;

public sealed partial class CurseForgePage : Page
{
    public CurseForgePage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetService<CurseForgeViewModel>();
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
