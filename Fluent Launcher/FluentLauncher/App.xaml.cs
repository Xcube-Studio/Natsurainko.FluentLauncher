using FluentLauncher.Classes;
using FluentLauncher.Converters;
using FluentLauncher.DesktopBridger;
using FluentLauncher.Models;
using FluentLauncher.Pages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentLauncher
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            BeforeInitialized();

            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
        }

        public static AppDesktopBridgeContainer DesktopBridge { get; set; } = new AppDesktopBridgeContainer();

        public static ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings;

        public static Task DesktopBridge_Init { get; set; }

        #region EVENT

        #region Application
        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            RegisterExceptionHandlingSynchronizationContext();

            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.Navigate(ShareResource.RunForFirstTime ? typeof(GuidePage) : typeof(MainContainer), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }

            AfterOnLaunched();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            RegisterExceptionHandlingSynchronizationContext();
            base.OnActivated(args);
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            DesktopBridge.OnBackgroundActivated(args);
        }
        #endregion

        private void AfterOnLaunched()
        {
            var theme = (Models.ApplicationTheme)this.Resources["ApplicationTheme"];

            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonBackgroundColor = Colors.Transparent;
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            if (App.Current.RequestedTheme == Windows.UI.Xaml.ApplicationTheme.Dark)
            {
                theme.SystemBackgroundColor = Colors.Black;
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = Colors.White;
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonHoverBackgroundColor = ColorConverter.FromString("#19FFFFFF");
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonPressedBackgroundColor = ColorConverter.FromString("#3FFFFFFF");
            }
            else
            {
                theme.SystemBackgroundColor = Colors.White;
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonHoverForegroundColor = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonPressedForegroundColor = Colors.Black;
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonHoverBackgroundColor = ColorConverter.FromString("#19000000");
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar.ButtonPressedBackgroundColor = ColorConverter.FromString("#3F000000");
            }

            this.Resources["ApplicationTheme"] = theme;
        }

        private void BeforeInitialized()
        {
            DesktopBridge_Init = DesktopBridge.BeginInitAsync();
            LoadSettings();
            if (ShareResource.RunForFirstTime)
                ShareResource.RunForFirstTimeTask = RunForFirstTime();

            LoadLanguage();

            if (!ShareResource.RunForFirstTime)
                Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.Auto;
        }

        private void LoadSettings()
        {
            var cache_account = new MinecraftAccount
            {
                Type = "OfflineAccount",
                AccessToken = "5627dd98e6be3c21b8a8e92344183641",
                Uuid = "5627dd98e6be3c21b8a8e92344183641",
                UserName = "Steve"
            };

            #region Data Setting
            ShareResource.MinecraftFolders = Settings.Values.ContainsKey("MinecraftFolders") && Settings.Values["MinecraftFolders"] != null ?
                JsonConvert.DeserializeObject<List<MinecraftFolder>>((string)Settings.Values["MinecraftFolders"]) : new List<MinecraftFolder>();

            ShareResource.SelectedFolder = Settings.Values.ContainsKey("SelectedFolder") && Settings.Values["SelectedFolder"] != null ?
                JsonConvert.DeserializeObject<MinecraftFolder>((string)Settings.Values["SelectedFolder"]) : null;

            ShareResource.JavaRuntimeEnvironments = Settings.Values.ContainsKey("JavaRuntimeEnvironments") && Settings.Values["JavaRuntimeEnvironments"] != null ?
                JsonConvert.DeserializeObject<List<JavaRuntimeEnvironment>>((string)Settings.Values["JavaRuntimeEnvironments"]) : new List<JavaRuntimeEnvironment>();

            ShareResource.SelectedJava = Settings.Values.ContainsKey("SelectedJava") && Settings.Values["SelectedJava"] != null ?
                JsonConvert.DeserializeObject<JavaRuntimeEnvironment>((string)Settings.Values["SelectedJava"]) : null;

            ShareResource.SelectedCore = Settings.Values.ContainsKey("SelectedCore") && Settings.Values["SelectedCore"] != null ?
                JsonConvert.DeserializeObject<MinecraftCoreInfo>((string)Settings.Values["SelectedCore"]) : null;

            ShareResource.MaxMemory = Settings.Values.ContainsKey("MaxMemory") && Settings.Values["MaxMemory"] != null ?
                (int)Settings.Values["MaxMemory"] : 1024;

            ShareResource.MinMemory = Settings.Values.ContainsKey("MinMemory") && Settings.Values["MinMemory"] != null ?
                (int)Settings.Values["MinMemory"] : 1024;

            ShareResource.MaxThreads = Settings.Values.ContainsKey("MaxThreads") && Settings.Values["MaxThreads"] != null ?
                (int)Settings.Values["MaxThreads"] : 64;

            ShareResource.WorkingFolder = Settings.Values.ContainsKey("WorkingFolder") && Settings.Values["WorkingFolder"] != null ?
                (string)Settings.Values["WorkingFolder"] : "Standard [.minecraft/]";

            ShareResource.DownloadSource = Settings.Values.ContainsKey("DownloadSource") && Settings.Values["DownloadSource"] != null ?
                (string)Settings.Values["DownloadSource"] : "Mcbbs Source";

            ShareResource.Language = Settings.Values.ContainsKey("Language") && Settings.Values["Language"] != null ?
                (string)Settings.Values["Language"] : "中文";

            ShareResource.ShownUpdate = Settings.Values.ContainsKey("ShownUpdate") && Settings.Values["ShownUpdate"] != null ?
                (string)Settings.Values["ShownUpdate"] : string.Empty;

            ShareResource.RunForFirstTime = Settings.Values.ContainsKey("RunForFirstTime") && Settings.Values["RunForFirstTime"] != null ?
                (bool)Settings.Values["RunForFirstTime"] : true;

            ShareResource.MinecraftAccounts = Settings.Values.ContainsKey("MinecraftAccounts") && Settings.Values["MinecraftAccounts"] != null ?
                JsonConvert.DeserializeObject<List<MinecraftAccount>>((string)Settings.Values["MinecraftAccounts"]) : new List<MinecraftAccount>() { cache_account };

            ShareResource.SelectedAccount = Settings.Values.ContainsKey("SelectedAccount") && Settings.Values["SelectedAccount"] != null ?
                JsonConvert.DeserializeObject<MinecraftAccount>((string)Settings.Values["SelectedAccount"]) : cache_account;
            #endregion

            #region UI Setting
            ShareResource.MainPageNewsVisibility = Settings.Values.ContainsKey("MainPageNewsVisibility") && Settings.Values["MainPageNewsVisibility"] != null ?
                (bool)Settings.Values["MainPageNewsVisibility"] : true;
            #endregion
        }

        private async void LoadLanguage()
        {
            switch (ShareResource.Language)
            {
                case "中文":
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "zh-CN";
                    break;
                case "English":
                    Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
                    break;
                default:
                    break;
            }

            ShareResource.LanguageResource = new Strings.BackgroundLanguageResource(ShareResource.Language);

            await DesktopBridge_Init;
            await DesktopBridge.SendAsync<StandardResponseModel>(new StandardResponseModel() { Header = "SetLanguage", Message = ShareResource.Language });
        }

        private async Task RunForFirstTime()
        {
            #region Windows Size
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize = new Size(920, 475);
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchWindowingMode = Windows.UI.ViewManagement.ApplicationViewWindowingMode.PreferredLaunchViewSize;
            #endregion

            #region Language
            if (CultureInfo.CurrentUICulture.Name == "zh-CN")
                ShareResource.Language = "中文";
            else ShareResource.Language = "English";
            #endregion

            #region Search Java Runtime

            ShareResource.JavaRuntimeEnvironments = new List<JavaRuntimeEnvironment>();

            await DesktopBridge_Init;

            var res = await DesktopBridge.SendAsync<StandardResponseModel>(new StandardRequestModel()
            {
                Header = "SearchJavaRuntime"
            });

            if (res == null)
                return;

            var list = JsonConvert.DeserializeObject<List<JavaRuntimeEnvironment>>(res.Response);

            foreach (var item in list)
            {
                int version = await item.GetJavaVersionAsync();

                if (version == 8 || version == 17)
                {
                    ShareResource.JavaRuntimeEnvironments.AddWithUpdate(item);
                    ShareResource.SelectedJava = item;
                }
            }

            #endregion
        }

        private void RegisterExceptionHandlingSynchronizationContext() =>
            ExceptionHandlingSynchronizationContext.Register().UnhandledException += SynchronizationContext_UnhandledException;

        private async void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var infobulider = new StringBuilder();
            var stacktrace = new StackTrace(e.Exception, true).GetFrame(0);

            if (stacktrace != null)
                infobulider.AppendLine($"{stacktrace.GetMethod()} - {stacktrace.GetFileName()}[line:{stacktrace.GetFileLineNumber()},{stacktrace.GetFileColumnNumber()}]");

            if (!string.IsNullOrEmpty(e.Exception.Source))
                infobulider.AppendLine(e.Exception.Source);
            if (!string.IsNullOrEmpty(e.Exception.StackTrace))
                infobulider.AppendLine(e.Exception.StackTrace);

            await ShareResource.ShowInfoAsync(ShareResource.LanguageResource.App_Exception, infobulider.ToString().TrimEnd(), 5000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning);
        }

        private async void SynchronizationContext_UnhandledException(object sender, AysncUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            var infobulider = new StringBuilder();
            var stacktrace = new StackTrace(e.Exception, true).GetFrame(0);

            infobulider.AppendLine($"{stacktrace.GetMethod()} - {stacktrace.GetFileName()}[line:{stacktrace.GetFileLineNumber()},{stacktrace.GetFileColumnNumber()}]");
            infobulider.AppendLine(e.Exception.Source);
            infobulider.AppendLine(e.Exception.StackTrace);

            await ShareResource.ShowInfoAsync(ShareResource.LanguageResource.App_Exception, infobulider.ToString().TrimEnd(), 5000, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Warning);
        }

        #endregion
    }
}
