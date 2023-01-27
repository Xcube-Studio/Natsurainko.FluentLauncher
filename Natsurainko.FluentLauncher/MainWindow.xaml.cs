using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.Globalization;
using WinUIEx;

namespace Natsurainko.FluentLauncher;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
#if !MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        ApplicationLanguages.PrimaryLanguageOverride = App.Configuration.CurrentLanguage.Split(',')[0];
#endif

        InitializeComponent();

#if MICROSOFT_WINDOWSAPPSDK_SELFCONTAINED
        AppWindow.SetIcon(Path.Combine(Directory.GetCurrentDirectory(), "Assets/AppIcon.png"));
#else
        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.png"));
#endif

        AppWindow.Title = "Natsurainko.FluentLauncher";
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
        else Frame.Navigate(typeof(Views.Pages.Guides.Navigation));
    }
}
