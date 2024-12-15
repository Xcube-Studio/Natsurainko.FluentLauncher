using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Home;
using Windows.Foundation;
using Windows.UI;

namespace Natsurainko.FluentLauncher.Views.Home;

public sealed partial class HomePage : Page
{
    private readonly SettingsService _settingsService = App.GetService<SettingsService>();

    public HomePage()
    {
        this.InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var themeDictionaries = (App.Current.Resources.ThemeDictionaries[this.ActualTheme == ElementTheme.Light ? "Light" : "Dark"] as ResourceDictionary)!;

        if (_settingsService.UseHomeControlsMask)
        {
            Brush foregroundBrush = this.ActualTheme == ElementTheme.Light
                ? new SolidColorBrush(Color.FromArgb(255, 26, 26, 26))
                : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            LaunchButton.Shadow = SharedShadow;
            LaunchButton.Translation += new System.Numerics.Vector3(0, 0, 16);

            DropDownButton.Shadow = SharedShadow;
            DropDownButton.Translation += new System.Numerics.Vector3(0, 0, 16);
            DropDownButtonArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;

            HyperlinkButton.Shadow = SharedShadow;
            HyperlinkButton.Translation += new System.Numerics.Vector3(0, 0, 16);
            HyperlinkButtonArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;
            HyperlinkButton.Foreground = foregroundBrush;

            this.ActualThemeChanged += (_, e) =>
            {
                var themeDictionaries = (App.Current.Resources.ThemeDictionaries[this.ActualTheme == ElementTheme.Light ? "Light" : "Dark"] as ResourceDictionary)!;

                Brush foregroundBrush = this.ActualTheme == ElementTheme.Light
                    ? new SolidColorBrush(Color.FromArgb(255, 26, 26, 26))
                    : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                DropDownButtonArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;

                HyperlinkButtonArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;
                HyperlinkButton.Foreground = foregroundBrush;
            };
        }

        LaunchButton.Focus(FocusState.Programmatic);
    }

    private void Flyout_Opened(object sender, object e)
    {
        var vm = (HomeViewModel)DataContext;
        listView.ScrollIntoView(vm.ActiveMinecraftInstance);
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
