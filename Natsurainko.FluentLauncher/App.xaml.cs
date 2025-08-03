using FluentLauncher.Infra.UI.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Views;
using Natsurainko.FluentLauncher.Views.Dialogs;
using System;
using System.Linq;
using System.Text;
using Windows.System.UserProfile;

namespace Natsurainko.FluentLauncher;

public partial class App : Application
{
    private readonly IActivationService _activationService;

    public App(IActivationService activationService, ILogger<App> logger)
    {
        _activationService = activationService;

        InitializeComponent();

        Logger = logger;
        DispatcherQueue = DispatcherQueue.GetForCurrentThread();

        ConfigureExceptionHandling();
        ConfigureLanguage();

        GetService<MessengerService>().SubscribeEvents();
        GetService<AppearanceService>().RegisterApp(this);
        GetService<QuickLaunchService>().CleanRemovedJumpListItem();
    }

    static void ConfigureLanguage()
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
        Logger.ConfiguredLanguage(selectedLangCode);
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var mainInstance = AppInstance.FindOrRegisterForKey("FluentLauncher.Process.Main");

        if (mainInstance.IsCurrent)
        {
            mainInstance.Activated += (sender, e) =>
            {
                // Bring the main window to the foreground
                MainWindow.SwitchToThisWindow();

                // Handle the activation arguments
                if (e.Data is Windows.ApplicationModel.Activation.LaunchActivatedEventArgs redirectedArgs)
                {
                    Logger.RedirectedActivationFrom(redirectedArgs);
                    GetService<QuickLaunchService>().LaunchFromActivatedEventArgs(redirectedArgs.Arguments.Split(' '));
                }
            };

#if ENABLE_LOAD_EXTENSIONS

        global::FluentLauncher.Infra.ExtensionHost.ApplicationExtensionHost.Current.Initialize(this);

        foreach (var assembly in App.GetService<global::System.Collections.Generic.List<global::FluentLauncher.Infra.ExtensionHost.Assemblies.IExtensionAssembly>>())
            await assembly.LoadAsync();

        foreach (var extension in App.GetService<global::System.Collections.Generic.List<global::FluentLauncher.Infra.ExtensionHost.Extensions.IExtension>>())
            extension.SetServiceProvider(Services);

#endif
            _activationService.ActivateWindow("MainWindow");
        }
        else
        {
            var appInstance = AppInstance.GetCurrent();

            Logger.RedirectingActivationTo(appInstance.ProcessId);
            await mainInstance.RedirectActivationToAsync(appInstance.GetActivatedEventArgs());
            this.Exit();
        }
    }

    public static T GetService<T>() where T : notnull // TODO: Rename to GetRequiredService
        => Services.GetRequiredService<T>();
}

/// <summary>
/// Properties of the application.
/// </summary>
partial class App
{
    public static IServiceProvider Services => Program.AppHost.Services;

    public static MainWindow MainWindow { get; set; } = null!;

    public static DispatcherQueue DispatcherQueue { get; private set; } = null!;

    public static ILogger<App> Logger { get; private set; } = null!;

    public static Windows.ApplicationModel.PackageVersion Version => Windows.ApplicationModel.Package.Current.Id.Version;

#if FLUENT_LAUNCHER_DEV_CHANNEL
    public static string AppChannel => LocalizedStrings.Settings_AboutPage__DevChannel;
#elif FLUENT_LAUNCHER_PREVIEW_CHANNEL
    public static string AppChannel => LocalizedStrings.Settings_AboutPage__PreviewChannel;
#elif FLUENT_LAUNCHER_STABLE_CHANNEL
    public static string AppChannel => LocalizedStrings.Settings_AboutPage__StableChannel;
#endif
}

/// <summary>
/// Exception handling for the application.
/// </summary>
partial class App
{
    /// <summary>
    /// Configure the exception handling for the application.
    /// </summary>
    void ConfigureExceptionHandling()
    {
        UnhandledException += (_, e) =>
        {
            bool isUserInterfaceAvailable = DispatcherQueue is not null &&
                App.MainWindow is not null &&
                MainWindow.XamlRoot is not null &&
                e.Message != "Layout cycle detected.  Layout could not complete.";

            // If the App.MainWindow is null, it means the exception is thrown before the main window is created,
            // and the window won't be created again. So we can't recover from this exception.

            bool isRecoverable = App.MainWindow is not null &&
                e.Message != "Layout cycle detected.  Layout could not complete.";

            ProcessException(e.Exception, isUserInterfaceAvailable, isRecoverable);
            e.Handled = isRecoverable;

            // If the exception is not recoverable, we need to exit the application.
            if (!isRecoverable) this.Exit();
        };
    }

    /// <summary>
    /// Process an unhandled exception in the application.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="isUIAvailable"></param>
    /// <param name="isRecoverable"></param>
    public static void ProcessException(Exception ex, bool isUIAvailable, bool isRecoverable)
    {
        Logger.UnhandledException(ex, isUIAvailable, isRecoverable);

        if (isUIAvailable) ShowExceptionWithContentDialog(ex);
        else ShowExceptionWithMessageBox(ex);
    }

    /// <summary>
    /// When dispatcher queue is not available, show exception in a message box
    /// </summary>
    /// <param name="ex"></param>
    static void ShowExceptionWithMessageBox(Exception exception)
    {
        string title = "The application encountered an unrecoverable error";
        string errorMessage = GetErrorMessage(exception);

        System.Windows.MessageBox.Show(
            errorMessage,
            title,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error,
            System.Windows.MessageBoxResult.OK);
    }

    /// <summary>
    /// When dispatcher queue is available, show exception in a content dialog
    /// </summary>
    /// <param name="ex"></param>
    static void ShowExceptionWithContentDialog(Exception exception)
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            try
            {
                await new ExceptionDialog(GetErrorMessage(exception)).ShowAsync();
            }
            catch (Exception ex)
            {
                // If the dialog cannot be shown, fallback to a message box
                ShowExceptionWithMessageBox(new AggregateException(exception, ex));
            }
        });
    }

    /// <summary>
    /// Format the error message from an exception, including its inner exceptions.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="callCount"></param>
    /// <returns></returns>
    static string GetErrorMessage(Exception e, int callCount = 0)
    {
        if (e is null) return string.Empty;
        if (callCount > 5) // Prevent infinite recursion
            return "\r\nToo many nested exceptions, unable to display.";

        string indent = new(' ', callCount * 2);
        StringBuilder sb = new ($"{indent}{e.GetType().FullName}\r\n");

        if (e is AggregateException aggregateException)
        {
            sb.AppendLine($"{indent}AggregateException.InnerExceptions:");

            foreach (var inner in aggregateException.InnerExceptions)
                sb.AppendLine(GetErrorMessage(inner, callCount + 1));
        }
        else 
        {
            sb.AppendLine($"{indent}Message: {e.Message}");
            sb.AppendLine($"{indent}Source: {e.Source}");
            sb.AppendLine($"{indent}TargetSite: {e.TargetSite}");
            sb.AppendLine($"{indent}StackTrace: {e.StackTrace}");

            if (e.Data != null && e.Data.Count > 0)
            {
                sb.AppendLine($"{indent}Data:");

                foreach (var key in e.Data.Keys)
                    sb.AppendLine($"{indent}  {key}: {e.Data[key]}");
            }

            if (e.InnerException != null)
            {
                sb.AppendLine($"{indent}InnerException:");
                sb.AppendLine(GetErrorMessage(e.InnerException, callCount + 1));
            }
        }

        return sb.ToString();
    }
}

internal static partial class AppLoggers
{
    [LoggerMessage(LogLevel.Information, "Configured launguage {languageCode} for application")]
    public static partial void ConfiguredLanguage(this ILogger logger, string languageCode);

    [LoggerMessage(LogLevel.Error, "Unhandled exception occurred, [\"isUserInterfaceAvailable\": {isUserInterfaceAvailable}, \"isRecoverable\": {isRecoverable}]")]
    public static partial void UnhandledException(this ILogger logger, Exception exception, bool isUserInterfaceAvailable, bool isRecoverable);

    [LoggerMessage(LogLevel.Information, "AppInstance [\"processId\": {processId}] is not the main AppInstance, redirecting activation and exiting..")]
    public static partial void RedirectingActivationTo(this ILogger logger, uint processId);

    [LoggerMessage(LogLevel.Information, "Redirected activation from the other instance with EventArgs {@eventArgs}")]
    public static partial void RedirectedActivationFrom(this ILogger logger, Windows.ApplicationModel.Activation.LaunchActivatedEventArgs eventArgs);
}