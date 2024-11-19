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
    private readonly ThemeShadow _themeShadow = new();
    private readonly SettingsService _settingsService = App.GetService<SettingsService>();

    public HomePage()
    {
        this.InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        _themeShadow.Receivers.Add(Grid);

        if (_settingsService.UseHomeControlsMask)
        {
            Brush maskColorBrush = this.ActualTheme == ElementTheme.Light
                    ? new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                    : new SolidColorBrush(Color.FromArgb(76, 58, 58, 58));

            Brush foregroundBrush = this.ActualTheme == ElementTheme.Light
                ? new SolidColorBrush(Color.FromArgb(255, 26, 26, 26))
                : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            DropDownButton.Shadow = _themeShadow;
            DropDownButton.Translation += new System.Numerics.Vector3(0, 0, 32);
            DropDownButtonArea.Background = new CommunityToolkit.WinUI.Media.BackdropBlurBrush() { Amount = 16 };
            DropDownButton.Background = maskColorBrush;

            HyperlinkButton.Shadow = _themeShadow;
            HyperlinkButton.Translation += new System.Numerics.Vector3(0, 0, 32);
            HyperlinkButtonArea.Background = new CommunityToolkit.WinUI.Media.BackdropBlurBrush() { Amount = 16 };
            HyperlinkButton.Background = maskColorBrush;
            HyperlinkButton.Foreground = foregroundBrush;

            this.ActualThemeChanged += (_, e) =>
            {
                Brush maskColorBrush = this.ActualTheme == ElementTheme.Light
                    ? new SolidColorBrush(Color.FromArgb(128, 255, 255, 255))
                    : new SolidColorBrush(Color.FromArgb(76, 58, 58, 58));

                Brush foregroundBrush = this.ActualTheme == ElementTheme.Light
                    ? new SolidColorBrush(Color.FromArgb(255, 26, 26, 26))
                    : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                DropDownButton.Background = maskColorBrush;
                HyperlinkButton.Background = maskColorBrush;
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
