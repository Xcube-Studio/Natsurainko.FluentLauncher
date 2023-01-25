using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Views.Pages;
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

        Frame.Navigate(typeof(MainContainer));
    }
}
