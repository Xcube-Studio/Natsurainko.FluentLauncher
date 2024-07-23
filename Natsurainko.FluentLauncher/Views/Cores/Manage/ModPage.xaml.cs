using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.Views.Cores.Manage;

public sealed partial class ModPage : Page, IBreadcrumbBarAware
{
    public string Route => "Mod";

    public ModPage()
    {
        this.InitializeComponent();
    }

    private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var grid = (sender as Grid)!;
        var textBlock = (grid.FindName("InfoText") as TextBlock)!;

        textBlock.TextWrapping = Microsoft.UI.Xaml.TextWrapping.WrapWholeWords;
        textBlock.TextTrimming = Microsoft.UI.Xaml.TextTrimming.None;
        textBlock.MaxLines = 0;
    }

    private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var grid = (sender as Grid)!;
        var textBlock = (grid.FindName("InfoText") as TextBlock)!;

        textBlock.TextWrapping = Microsoft.UI.Xaml.TextWrapping.NoWrap;
        textBlock.TextTrimming = Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis;
        textBlock.MaxLines = 1;
    }
}
