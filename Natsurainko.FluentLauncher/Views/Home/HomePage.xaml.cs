using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Home;
using Nrk.FluentCore.Authentication;
using System;
using Windows.Foundation;
using Windows.UI;

namespace Natsurainko.FluentLauncher.Views.Home;

public sealed partial class HomePage : Page
{
    private readonly SettingsService _settingsService = App.GetService<SettingsService>();

    HomeViewModel VM => (HomeViewModel)DataContext;

    public HomePage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var themeDictionaries = (App.Current.Resources.ThemeDictionaries[this.ActualTheme == ElementTheme.Light ? "Light" : "Dark"] as ResourceDictionary)!;

        if (_settingsService.UseHomeControlsMask)
        {
            Brush foregroundBrush = this.ActualTheme == ElementTheme.Light
                ? new SolidColorBrush(Color.FromArgb(255, 26, 26, 26))
                : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            LaunchButton.Translation += new System.Numerics.Vector3(0, 0, 16);
            InstanceSelectorArea.Translation += new System.Numerics.Vector3(0, 0, 16);
            AccountSelectorArea.Translation += new System.Numerics.Vector3(0, 0, 16);
            LaunchingInfoArea.Translation += new System.Numerics.Vector3(0, 0, 16);

            InstanceSelectorArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;
            AccountSelectorArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;
            LaunchingInfoArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;

            AccountSelectorButton.Foreground = foregroundBrush;

            this.ActualThemeChanged += (_, e) =>
            {
                var themeDictionaries = (App.Current.Resources.ThemeDictionaries[this.ActualTheme == ElementTheme.Light ? "Light" : "Dark"] as ResourceDictionary)!;

                Brush foregroundBrush = this.ActualTheme == ElementTheme.Light
                    ? new SolidColorBrush(Color.FromArgb(255, 26, 26, 26))
                    : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                InstanceSelectorArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;
                AccountSelectorArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;
                LaunchingInfoArea.Background = themeDictionaries["NavigationViewUnfoldedPaneBackground"] as AcrylicBrush;

                AccountSelectorButton.Foreground = foregroundBrush;
            };
        }

        if (_settingsService.HomeLaunchButtonSize == 1)
        {
            LaunchButtonIcon.FontSize = 18;
            LaunchButton.FontSize = 16;

            LaunchButton.VerticalAlignment = VerticalAlignment.Stretch;
        }

        InstanceSelectorGrid.TranslationTransition = new Vector3Transition()
        {
            Duration = TimeSpan.FromMilliseconds(500)
        };
        LaunchingInfoGrid.TranslationTransition = new Vector3Transition()
        {
            Duration = TimeSpan.FromMilliseconds(500)
        };

        InstanceSelectorGrid.OpacityTransition = new ScalarTransition()
        {
            Duration = TimeSpan.FromMilliseconds(250)
        };
        LaunchingInfoGrid.OpacityTransition = new ScalarTransition()
        {
            Duration = TimeSpan.FromMilliseconds(250)
        };

        LaunchButton.Focus(FocusState.Programmatic);
    }

    private void Flyout_Opened(object sender, object e)
    {
        var vm = (HomeViewModel)DataContext;
        listView.ScrollIntoView(vm.ActiveMinecraftInstance);
    }

    private void HideAccountFlyoutHandler(object sender, RoutedEventArgs e)
    {
        accountSelectorFlyout.Hide();
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

    #region Converters Methods

    internal static string GetAccountTypeName(AccountType accountType)
    {
        string account = LocalizedStrings.Converters__Account;

        if (!ApplicationLanguages.PrimaryLanguageOverride.StartsWith("zh-"))
            account = " " + account;

        return accountType switch
        {
            AccountType.Microsoft => LocalizedStrings.Converters__Microsoft + account,
            AccountType.Yggdrasil => LocalizedStrings.Converters__Yggdrasil + account,
            _ => LocalizedStrings.Converters__Offline + account,
        };
    }

    internal static string TryGetYggdrasilServerName(Account account)
    {
        if (account is YggdrasilAccount yggdrasilAccount)
        {
            if (yggdrasilAccount.MetaData.TryGetValue("server_name", out var serverName))
                return serverName;
        }

        return string.Empty;
    }

    #endregion
}
