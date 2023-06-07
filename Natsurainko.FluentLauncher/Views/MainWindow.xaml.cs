using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Settings;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Globalization;
using WinUIEx;

namespace Natsurainko.FluentLauncher.Views;

public sealed partial class MainWindow : WindowEx
{
    public Frame ContentFrame => Frame;

    private readonly SettingsService _settings = App.GetService<SettingsService>();

    public MainWindow()
    {
#if !MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        if (string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride))
            LanguageResources.ApplyLanguage(_settings.CurrentLanguage);
#endif

        InitializeComponent();

        MessageService.RegisterContainer(MessageList);

#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        AppWindow.SetIcon(Path.Combine(Directory.GetCurrentDirectory(), "Assets/AppIcon.png"));
#else
        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.png"));
#endif

        AppWindow.Title = "Fluent Launcher";
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        
        (MinWidth, MinHeight) = (516, 328);
        (Width, Height) = (_settings.AppWindowWidth, _settings.AppWindowHeight);

        SystemBackdrop = Environment.OSVersion.Version.Build >= 22000
           ? new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt }
           : new DesktopAcrylicBackdrop();

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
