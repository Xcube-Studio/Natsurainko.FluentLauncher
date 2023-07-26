using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Classes.Data.UI;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using System.IO;
using Windows.ApplicationModel;
using Windows.Globalization;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class MainWindow : WindowEx
{
    public Frame ContentFrame => Frame;

    private readonly SettingsService _settings = App.GetService<SettingsService>();
    private readonly NotificationService _notificationService = App.GetService<NotificationService>();

    public MainWindow()
    {
        if (string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride))
            ResourceUtils.ApplyLanguage(_settings.CurrentLanguage);

        InitializeComponent();

        MessageService.RegisterContainer(MessageList);
        _notificationService.InitContainer(NotifyStackPanel, BackgroundGrid);

        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.ico"));

        AppWindow.Title = "Fluent Launcher";
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonForegroundColor = App.Current.RequestedTheme == ApplicationTheme.Light 
            ? Colors.Black : Colors.White;

        (MinWidth, MinHeight) = (516, 328);
        (Width, Height) = (_settings.AppWindowWidth, _settings.AppWindowHeight);

        if (_settings.FinishGuide)
            Frame.Navigate(typeof(ShellPage));
        else Frame.Navigate(typeof(OOBE.OOBENavigationPage));
    }

    private void InfoBar_CloseButtonClick(InfoBar sender, object args)
    {
        var messageData = sender.DataContext as MessageData;
        messageData.Removed = true;

        MessageList.Items.Remove(messageData);
    }

    private void InfoBar_Loaded(object sender, RoutedEventArgs e) => ((InfoBar)sender).Translation += new System.Numerics.Vector3(0, 0, 32);
}
