using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Utils;
using ReverseMarkdown.Converters;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Windows.System.UserProfile;

namespace Natsurainko.FluentLauncher;

public partial class App : Application
{
    public static IServiceProvider Services => Program.AppHost.Services;

    public static MainWindow MainWindow { get; set; } = null!;

    public static DispatcherQueue DispatcherQueue { get; private set; } = null!;

    public static Windows.ApplicationModel.PackageVersion Version => Windows.ApplicationModel.Package.Current.Id.Version;

#if FLUENT_LAUNCHER_DEV_CHANNEL
    public static string AppChannel => LocalizedStrings.Settings_AboutPage__DevChannel;
#elif FLUENT_LAUNCHER_PREVIEW_CHANNEL
    public static string AppChannel => LocalizedStrings.Settings_AboutPage__PreviewChannel;
#elif FLUENT_LAUNCHER_STABLE_CHANNEL
    public static string AppChannel => LocalizedStrings.Settings_AboutPage__StableChannel;
#endif

    public App()
    {
        InitializeComponent();
        ConfigureApplication();
    }

    void ConfigureApplication()
    {
        //Fix https://github.com/MCLF-CN/docs/issues/2 
        HttpUtils.HttpClient.DefaultRequestHeaders.UserAgent.Clear();
        HttpUtils.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Natsurainko.FluentLauncher", Version.GetVersionString()));

        DispatcherQueue = DispatcherQueue.GetForCurrentThread();

        App.GetService<MessengerService>().SubscribeEvents();
        App.GetService<AppearanceService>().RegisterApp(this);
        App.GetService<QuickLaunchService>().CleanRemovedJumpListItem();

        ConfigureLanguage();

        // Global exception handler
        UnhandledException += (_, e) =>
        {
            e.Handled = true;
            ProcessException(e.Exception);
        };
    }

    private void ConfigureLanguage()
    {
        var settings = App.GetService<SettingsService>();
        var selectedLangCode = settings.CurrentLanguage;

        // Choose language using system language preference on first launch
        if (selectedLangCode == "")
        {
            foreach (string langCode in GlobalizationPreferences.Languages)
            {
                // Match the language preference with supported languages
                // StartsWith is used to match the language code with the region code, for example "zh-hans-CN" with "zh-Hans"
                var suitableLanguages = LocalizedStrings.SupportedLanguages.Where(x => langCode.StartsWith(x.LanguageCode));
                if (suitableLanguages.Any())
                {
                    // Store a LanguageCode in LocalizedStrings.SupportedLanguages ​​for conversion by LanguageCodeToLanguageInfoConverter.
                    // Storing langCode directly, such as "zh-Hans-CN", will cause Converter to throw an exception.

                    selectedLangCode = suitableLanguages.First().LanguageCode;
                    settings.CurrentLanguage = selectedLangCode;
                    break;
                }
            }

            // Fall back to English if no match
            if (selectedLangCode == "")
                selectedLangCode = "en-US";
        }

        // Apply the language
        LocalizedStrings.ApplyLanguage(selectedLangCode);
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // 确保单例应用程序启动
        var mainInstance = AppInstance.FindOrRegisterForKey("Main");
        mainInstance.Activated += (object? sender, AppActivationArguments e) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                MainWindow?.Activate();
                MainWindow?.BringToFront();
            });

            if (e.Data is Windows.ApplicationModel.Activation.LaunchActivatedEventArgs redirectedArgs)
                App.GetService<QuickLaunchService>().LaunchFromActivatedEventArgs(redirectedArgs.Arguments.Split(' '));
        };

        if (!mainInstance.IsCurrent)
        {
            //Redirect the activation (and args) to the "main" instance, and exit.
            var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

            await mainInstance.RedirectActivationToAsync(activatedEventArgs);
            Process.GetCurrentProcess().Kill();
            return;
        }

        try
        {
            IWindowService mainWindowService = App.GetService<IActivationService>().ActivateWindow("MainWindow");
        }
        catch (Exception e)
        {
            ProcessException(e);
        }
    }

    #region Global exception handlers

    public static string GetErrorMessage(Exception e)
    {
        if (e is null) return string.Empty;

        var stringBuilder = new StringBuilder()
            .AppendLine(e.GetType().FullName)
            .AppendLine(e.ToString());

        if (e.InnerException != null)
        {
            stringBuilder.AppendLine("InnerException:");
            stringBuilder.AppendLine(e.InnerException.ToString());
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Show error message in an appropriate way
    /// </summary>
    /// <param name="errorMessage"></param>
    public static void ShowErrorMessage(string errorMessage)
    {
        if (App.MainWindow is not null)
        {
            App.DispatcherQueue?.TryEnqueue(async () =>
            {
                try
                {
                    await new ExceptionDialog(errorMessage) { XamlRoot = MainWindow.XamlRoot }.ShowAsync();
                }
                catch
                {
                    var window = new Window() { Title = "Fluent Launcher" };
                    window.Content = new ExceptionPage(errorMessage);
                    window.Activate();
                }
            });
        }
        else
        {
            var window = new Window() { Title = "Fluent Launcher" };
            window.Content = new ExceptionPage(errorMessage);
            window.Activate();
        }
    }

    public static void ProcessException(Exception ex)
    {
        var errorMessage = GetErrorMessage(ex);
        // TODO: Log the error message

        ShowErrorMessage(errorMessage);
    }

    #endregion

    public static T GetService<T>() where T : notnull // TODO: Rename to GetRequiredService
        => Services.GetRequiredService<T>();
}
