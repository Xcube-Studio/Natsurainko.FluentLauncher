using Natsurainko.FluentCore.Class.Model.Launch;
using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Shared.Mapping;
using Natsurainko.FluentLauncher.View.Pages;
using Natsurainko.FluentLauncher.View.Pages.Activities;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Windows.ApplicationModel.Core;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class LauncherProcessViewData : ViewDataBase<LauncherProcess>
{
    public LauncherProcessViewData(LauncherProcess data) : base(data)
    {
        data.StateChanged += Data_StateChanged;
        data.Exited += Data_Exited;
        data.LaunchFailed += Data_LaunchFailed;

        Title = ConfigurationManager.AppSettings.CurrentLanguage.GetString("MinecraftProcessor_Title").Replace("{Id}", data.GameCore.Id);
    }

    [Reactive]
    public ObservableCollection<string> Outputs { get; private set; }

    [Reactive]
    public IEnumerable<string> Arguments { get; private set; }

    [Reactive]
    public string Title { get; private set; }

    [Reactive]
    public string State { get; private set; }

    [Reactive]
    public string ExitedMessage { get; private set; }

    [Reactive]
    public string ExitedDescription { get; private set; }

    [Reactive]
    public string RunTime { get; private set; }

    [Reactive]
    public string LaunchException { get; private set; }

    [Reactive]
    public string LaunchExceptionSource { get; private set; }

    [Reactive]
    public string LaunchExceptionStackTrace { get; private set; }

    [Reactive]
    public string LaunchExceptionMessage { get; private set; }

    [Reactive]
    public Visibility StopButtonVisibility { get; private set; } = Visibility.Collapsed;

    [Reactive]
    public bool IsExpanded { get; private set; } = true;

    private Timer Timer;

    public async Task<bool> Launch()
    {
        var @bool = await Data.LaunchAsync();

        if (@bool)
        {
            DispatcherHelper.RunAsync(() =>
            {
                StopButtonVisibility = Visibility.Visible;
                Outputs = Data.Outputs;
                Arguments = Data.Arguments;
            });

            Timer = new Timer(1000);
            Timer.Start();
            Timer.Elapsed += Timer_Elapsed;
        }

        return @bool;
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e) => DispatcherHelper.RunAsync(() => RunTime = (DateTime.Now - Data.ProcessStartTime).ToString(@"hh\:mm\:ss"));

    private void Data_StateChanged(object sender, string e) => DispatcherHelper.RunAsync(() => State = e);

    private void Data_Exited(object sender, FluentCore.Event.MinecraftExitedArgs e)
    {
        Timer.Stop();

        DispatcherHelper.RunAsync(() =>
        {
            ExitedMessage = ConfigurationManager.AppSettings.CurrentLanguage.GetString($"MinecraftProcessor_ExitedMessage_{(e.Crashed ? 1 : 2)}");
            ExitedDescription = ConfigurationManager.AppSettings.CurrentLanguage.GetString($"MinecraftProcessor_ExitedDescription").Replace("{code}", e.ExitCode.ToString());
            RunTime = (DateTime.Now - Data.ProcessStartTime).ToString(@"hh\:mm\:ss");
            IsExpanded = false;
            StopButtonVisibility = Visibility.Collapsed;
        });
    }

    private void Data_LaunchFailed(object sender, Newtonsoft.Json.Linq.JObject e)
    {
        DispatcherHelper.RunAsync(() =>
        {
            LaunchException = e["ClassName"].ToString();
            LaunchExceptionSource = e["Source"].ToString();
            LaunchExceptionStackTrace = e["StackTraceString"].ToString();
            LaunchExceptionMessage = e["Message"].ToString();
        });
    }

    public async Task<AppWindow> CreateProcessOutputWindow()
    {
        AppWindow appWindow = null;

        await CoreApplication.MainView.Dispatcher.RunAsync(default, async delegate
        {
            appWindow = await AppWindow.TryCreateAsync();
            appWindow.Title = ConfigurationManager.AppSettings.CurrentLanguage.GetString("CreateProcessOutputWindow_Title").Replace("{Id}", this.Data.GameCore.Id);

            var appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(ProcessOutputPage), (appWindow, this));

            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);

            appWindow.Closed += delegate
            {
                appWindowContentFrame.Content = null;
                appWindow = null;
            };

            _ = appWindow.TryShowAsync();
        });

        return appWindow;
    }

    public static async void CreateLaunchProcess(GameCore gameCore, Control control)
    {
        control.IsEnabled = false;

        var launcherProcess = new LauncherProcess(gameCore).CreateViewData<LauncherProcess, LauncherProcessViewData>();

        CacheResources.LauncherProcesses.Insert(0, launcherProcess);

        var hyperlinkButton = new HyperlinkButton { Content = ConfigurationManager.AppSettings.CurrentLanguage.GetString("Info_Launch_L") };
        hyperlinkButton.Click += (_, _) => MainContainer.ContentFrame.Navigate(typeof(ActivitiesPage), typeof(ActivityLaunchPage));

        MainContainer.ShowInfoBarAsync(
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Info_Launch_T").Replace("{Id}", gameCore.Id),
            ConfigurationManager.AppSettings.CurrentLanguage.GetString("Info_Launch_ST"),
            button: hyperlinkButton);

        await launcherProcess.Launch();

        control.IsEnabled = true;
    }
}
