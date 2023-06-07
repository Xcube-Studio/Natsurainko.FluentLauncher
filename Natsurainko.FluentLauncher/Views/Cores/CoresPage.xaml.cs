using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Cores;
using System;
using System.Linq;

namespace Natsurainko.FluentLauncher.Views.Cores;

public sealed partial class CoresPage : Page
{
    public CoresPage()
    {
        InitializeComponent();
        DataContext = App.GetService<CoresViewModel>();
    }

    private void Border_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        void CommandRefresh()
        {
            var border = (Border)sender;
            var father = (Grid)border.FindName("FatherGrid");
            var grid = (Grid)border.FindName("NameGrid");
            var width = father.ActualWidth - grid.ActualWidth;

            var stackPanel = (StackPanel)border.FindName("StackPanel");
            var menuFlyout = (MenuFlyout)stackPanel.FindName("MenuFlyout");

            var more = stackPanel.Children.Last();
            var others = stackPanel.Children.Take(stackPanel.Children.Count - 1).ToList();
            var visible = Math.Min((int)((width - 145) / 45), others.Count);

            if (visible < 0)
                return;

            if (visible == 4)
            {
                visible += 1;
                more.Visibility = Visibility.Collapsed;
            }
            else more.Visibility = width > 325
                ? Visibility.Collapsed
                : Visibility.Visible;

            for (int i = 0; i < visible; i++)
            {
                others[i].Visibility = Visibility.Visible;
                var text = ToolTipService.GetToolTip(others[i]) as TextBlock;

                menuFlyout.Items.Where(x => x.Tag.Equals(text.Text))
                    .ToList().ForEach(x => menuFlyout.Items.Remove(x));
            }

            others.Reverse();

            for (int i = 0; i < others.Count - visible; i++)
            {
                others[i].Visibility = Visibility.Collapsed;
                var text = ToolTipService.GetToolTip(others[i]) as TextBlock;

                if (!menuFlyout.Items.Where(x => x.Tag.Equals(text.Text)).Any())
                {
                    var icon = (others[i] as Button).Content as FontIcon;

                    menuFlyout.Items.Insert(0, new MenuFlyoutItem
                    {
                        Icon = new FontIcon() { Glyph = icon.Glyph, FontSize = icon.FontSize },
                        Text = text.Text,
                        Tag = text.Text,
                        Command = (others[i] as Button).Command,
                        CommandParameter = (others[i] as Button).CommandParameter
                    });
                }
                else
                {
                    var flyoutItem = menuFlyout.Items.First(x => x.Tag.Equals(text.Text)) as MenuFlyoutItem;

                    flyoutItem.Command = (others[i] as Button).Command;
                    flyoutItem.CommandParameter = (others[i] as Button).CommandParameter;
                }
            }
        }

        CommandRefresh();
        ((Grid)((Border)sender).FindName("ControlPanel")).Visibility = Visibility.Visible;
    }

    private void Border_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        => ((Grid)((Border)sender).FindName("ControlPanel")).Visibility = Visibility.Collapsed;

    private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        => InstallButtonText.Visibility = e.NewSize.Width > 655
            ? Visibility.Visible : Visibility.Collapsed;
}
