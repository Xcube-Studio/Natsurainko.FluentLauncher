using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher;

sealed partial class App : Application
{
    public App()
    {
        this.InitializeComponent();
        this.Suspending += OnSuspending;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs e)
    {
        ConfigurationManager.Configuration = new Configuration<AppSettings>(AppSettings.Default);
        await DesktopServiceManager.InitializeTask;

        Window.Current.Content = (Window.Current.Content as Frame) ?? new Frame();
        var rootFrame = Window.Current.Content as Frame;

        if (!e.PrelaunchActivated)
        {
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(MainContainer), e.Arguments);

            Window.Current.Activate();
        }
    }

    protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
    {
        base.OnBackgroundActivated(args);
        DesktopServiceManager.OnBackgroundActivated(args);
    }

    private void OnSuspending(object sender, SuspendingEventArgs e) => e.SuspendingOperation.GetDeferral().Complete();
}
