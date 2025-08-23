using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Notification;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

internal partial class OOBEViewModel : ObservableObject, INavigationAware, ISettingsViewModel
{
    [SettingsProvider]
    private readonly SettingsService _settings;
    private readonly GameService _gameService;
    private readonly AccountService _accountService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;
    private readonly INotificationService _notificationService;
    private readonly string OfficialLauncherPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\.minecraft";

    public INavigationService NavigationService { get; init; }

    public OOBEViewModel(
        INavigationService navigationService,
        SettingsService settings,
        GameService gameService,
        INotificationService notificationService,
        AccountService accountService,
        IDialogActivationService<ContentDialogResult> dialogs)
    {
        NavigationService = navigationService;
        _settings = settings;
        _gameService = gameService;
        _notificationService = notificationService;
        _accountService = accountService;
        _dialogs = dialogs;

        // Init accounts
        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;

        ((ISettingsViewModel)this).InitializeSettings();
    }

    #region Navigation

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    [NotifyPropertyChangedFor(nameof(NextText))]
    public partial int CurrentPageIndex { get; set; }

    private static readonly string[] OOBEPageKeys =
    {
        "OOBELanguagePage",
        "OOBEMinecraftFolderPage",
        "OOBEJavaPage",
        "OOBEAccountPage",
        "OOBEGetStartedPage"
    };

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        NavigationService.NavigateTo("OOBELanguagePage"); // Default page
    }

    [RelayCommand(CanExecute = nameof(CanNext))]
    public void Next()
    {
        if (CurrentPageIndex == OOBEPageKeys.Length - 1)
        {
            Start();
            return;
        }

        CurrentPageIndex++;
        NavigationService.NavigateTo(OOBEPageKeys[CurrentPageIndex]);
        BackCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    bool CanNext() => CurrentPageIndex switch
    {
        // Language page
        0 => LocalizedStrings.SupportedLanguages.Select(lang => lang.LanguageCode).Contains(CurrentLanguage),
        // Minecraft folder page
        1 => !string.IsNullOrEmpty(ActiveMinecraftFolder),
        // Java page
        2 => !string.IsNullOrEmpty(ActiveJavaRuntime) &&
                Directory.Exists(ActiveMinecraftFolder) &&
                File.Exists(ActiveJavaRuntime),
        // Account page
        3 => ActiveAccount is not null,
        // Get started page
        4 => true,
        // Default
        _ => false,
    };

    [RelayCommand(CanExecute = nameof(CanBack))]
    public void Back()
    {
        CurrentPageIndex--;
        NavigationService.NavigateTo(OOBEPageKeys[CurrentPageIndex]);
        BackCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    bool CanBack()
    {
        // Cannot go back on the first page
        if (CurrentPageIndex == 0)
            return false;

        return true;
    }

    public void NavigateTo(int pageIndex)
    {
        CurrentPageIndex = pageIndex;
        NavigationService.NavigateTo(OOBEPageKeys[pageIndex]);
        BackCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Language

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [BindToSetting(Path = nameof(SettingsService.CurrentLanguage))]
    public partial string CurrentLanguage { get; set; }

    public string Version => App.Version.GetVersionString();
    public string AppChannel => App.AppChannel;

    partial void OnCurrentLanguageChanged(string oldValue, string newValue)
    {
        if (oldValue is not null) // oldValue is null at startup
            LocalizedStrings.ApplyLanguage(CurrentLanguage);
    }

    #endregion

    #region Minecraft folder

    [BindToSetting(Path = nameof(SettingsService.MinecraftFolders))]
    public ObservableCollection<string> MinecraftFolders { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    public partial string ActiveMinecraftFolder { get; set; }

    [RelayCommand]
    public async Task BrowseFolder()
    {
        FolderPicker folderPicker = new();

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker,
            WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));

        folderPicker.FileTypeFilter.Add("*");

        if (await folderPicker.PickSingleFolderAsync() is StorageFolder folder)
        {
            if (MinecraftFolders.Contains(folder.Path))
            {
                _notificationService.FolderExisted(folder.Path);
                return;
            }

            _gameService.AddMinecraftFolder(folder.Path);
            _notificationService.FolderAdded(folder.Path);
        }
    }

    [RelayCommand]
    public void DetectOfficialMinecraftFolder()
    {
        // Official launcher .minecraft folder not exist
        if (!Directory.Exists(OfficialLauncherPath))
        {
            _notificationService.OfficialFolderNotFound(OfficialLauncherPath);
            return;
        }

        // Already added
        if (MinecraftFolders.Contains(OfficialLauncherPath))
        {
            _notificationService.OfficialFolderExisted(OfficialLauncherPath);
            return;
        }

        // Add to list

        _gameService.AddMinecraftFolder(OfficialLauncherPath);
        _notificationService.OfficialFolderAdded(OfficialLauncherPath);
    }

    [RelayCommand]
    public void RemoveFolder(string folder) => _gameService.RemoveMinecraftFolder(folder);

    #endregion

    #region Java

    [BindToSetting(Path = nameof(SettingsService.Javas))]
    public ObservableCollection<string> JavaRuntimes { get; private set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ActiveJava))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    public partial string ActiveJavaRuntime { get; set; }

    [RelayCommand]
    public void BrowseJava()
    {
        var openFileDialog = new OpenFileDialog
        {
            Multiselect = false,
            Filter = "Javaw Executable File|javaw.exe|Java Executable File|java.exe"
        };

        if (openFileDialog.ShowDialog().GetValueOrDefault(false))
        {
            if (JavaRuntimes.Contains(openFileDialog.FileName))
            {
                _notificationService.JavaExisted(openFileDialog.FileName);
                return;
            }

            JavaRuntimes.Add(openFileDialog.FileName);
            OnPropertyChanged(nameof(JavaRuntimes));

            ActiveJavaRuntime = openFileDialog.FileName;
            _notificationService.JavaAdded(openFileDialog.FileName);
        }
    }

    [RelayCommand]
    public void SearchJava()
    {
        try
        {
            foreach (var java in JavaUtils.SearchJava())
                if (!JavaRuntimes.Contains(java))
                    JavaRuntimes.Add(java);

            ActiveJavaRuntime = JavaRuntimes.Any() ? JavaRuntimes[0] : null;

            OnPropertyChanged(nameof(JavaRuntimes));
            _notificationService.JavaSearched();
        }
        catch (Exception ex)
        {
            _notificationService.JavaSearchFailed(ex);
        }
    }

    [RelayCommand]
    public void RemoveJava(string java)
    {
        JavaRuntimes.Remove(java);
        ActiveJavaRuntime = JavaRuntimes.Any() ? JavaRuntimes[0] : null;
    }

    #endregion

    #region Account

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    public partial Account ActiveAccount { get; set; }

    public bool processingActiveAccountChangedMessage = false;

    partial void OnActiveAccountChanged(Account value)
    {
        if (!processingActiveAccountChangedMessage)
        {
            if (value is not null)
                _accountService.ActivateAccount(value);
        }
    }

    [RelayCommand]
    public async Task Login(Button parameter) => await _dialogs.ShowAsync("AuthenticationWizardDialog");

    [RelayCommand]
    public void RemoveAccount(Account account)
    {
        _accountService.RemoveAccount(account);
    }
    #endregion

    public string NextText
    {
        get
        {
            if (CurrentPageIndex == OOBEPageKeys.Length - 1)
                return LocalizedStrings.OOBE_OOBEViewModel__ButtonGetStarted;
            else
                return LocalizedStrings.OOBE_OOBEViewModel__ButtonNext;
        }
    }

    [RelayCommand]
    public void Start()
    {
        NavigationService.Parent?.NavigateTo("ShellPage");
        (App.MainWindow.MinWidth, App.MainWindow.MinHeight) = (516, 328);

        _settings.FinishGuide = true;
    }
}

static partial class OOBEViewModelNotifications
{
    [Notification<InfoBar>(Title = "Notifications__FolderExisted", Message = "{path}")]
    public static partial void FolderExisted(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__FolderAdded", Message = "$path", Type = NotificationType.Success)]
    public static partial void FolderAdded(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__OfficialFolderNotFound", Message = "{path}", Type = NotificationType.Error)]
    public static partial void OfficialFolderNotFound(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__OfficialFolderExisted", Message = "{path}")]
    public static partial void OfficialFolderExisted(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__OfficialFolderAdded", Message = "{path}", Type = NotificationType.Success)]
    public static partial void OfficialFolderAdded(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__JavaExisted", Message = "{path}")]
    public static partial void JavaExisted(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__JavaAdded", Message = "{path}", Type = NotificationType.Success)]
    public static partial void JavaAdded(this INotificationService notificationService, string path);

    [Notification<InfoBar>(Title = "Notifications__JavaSearched", Type = NotificationType.Success)]
    public static partial void JavaSearched(this INotificationService notificationService);

    [ExceptionNotification(Title = "Notifications__JavaSearchFailed")]
    public static partial void JavaSearchFailed(this INotificationService notificationService, Exception exception);
}