using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Models;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Globalization;
using WinUIEx;

namespace Natsurainko.FluentLauncher;

public sealed partial class MainWindow : WindowEx
{
    public Frame ContentFrame => Frame;

    public MainWindow()
    {
#if !MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        if (string.IsNullOrEmpty(ApplicationLanguages.PrimaryLanguageOverride))
            LanguageResources.ApplyLanguage(App.Configuration.CurrentLanguage);
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
        (Width, Height) = (App.Configuration.AppWindowWidth, App.Configuration.AppWindowHeight);

        Backdrop = Environment.OSVersion.Version.Build >= 22000
           ? new MicaSystemBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt }
           : new AcrylicSystemBackdrop()
           {
               DarkTintOpacity = 0.75,
               DarkLuminosityOpacity = 0.75,
               DarkTintColor = Colors.Black,
               DarkFallbackColor = Colors.Black
           };

        if (App.Configuration.FinishGuide)
            Frame.Navigate(typeof(Views.Pages.MainContainer));
        else Frame.Navigate(typeof(Views.OOBE.Navigation));
    }

    private void InfoBar_CloseButtonClick(InfoBar sender, object args)
    {
        var messageData = sender.DataContext as MessageData;
        messageData.Removed = true;

        MessageList.Items.Remove(messageData);
    }

    private void InfoBar_Loaded(object sender, RoutedEventArgs e) => ((InfoBar)sender).Translation += new System.Numerics.Vector3(0, 0, 32);
}
