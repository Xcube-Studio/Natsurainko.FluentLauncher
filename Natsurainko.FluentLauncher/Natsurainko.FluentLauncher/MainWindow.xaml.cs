using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Views.Pages;
using System.IO;
using Windows.ApplicationModel;
using WinUIEx;

namespace Natsurainko.FluentLauncher;

public sealed partial class MainWindow : WindowEx
{
    public MainWindow()
    {
        InitializeComponent();

        AppWindow.SetIcon(Path.Combine(Package.Current.InstalledLocation.Path, "Assets/AppIcon.png"));

        AppWindow.Title = "Natsurainko.FluentLauncher";
        AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        (MinWidth, MinHeight) = (516, 328);
        (Width, Height) = (App.Configuration.AppWindowWidth, App.Configuration.AppWindowHeight);

        Frame.Navigate(typeof(MainContainer));
    }
}
