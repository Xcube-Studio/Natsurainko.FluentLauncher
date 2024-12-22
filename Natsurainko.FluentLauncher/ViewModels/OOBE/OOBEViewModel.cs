using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Environment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

internal partial class OOBEViewModel : ObservableObject, INavigationAware, ISettingsViewModel
{
    #region Dependencies

    [SettingsProvider]
    private readonly SettingsService _settings;

    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;
    private readonly AccountService _accountService;
    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    #endregion

    public OOBEViewModel(
        INavigationService navigationService,
        SettingsService settings,
        GameService gameService,
        NotificationService notificationService,
        AccountService accountService,
        IDialogActivationService<ContentDialogResult> dialogs)
    {
        _navigationService = navigationService;
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
        _navigationService.NavigateTo("OOBELanguagePage"); // Default page
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
        _navigationService.NavigateTo(OOBEPageKeys[CurrentPageIndex]);
        BackCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    bool CanNext() => CurrentPageIndex switch
    {
        // Language page
        0 => ResourceUtils.Languages.Contains(CurrentLanguage),
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
        _navigationService.NavigateTo(OOBEPageKeys[CurrentPageIndex]);
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
        _navigationService.NavigateTo(OOBEPageKeys[pageIndex]);
        BackCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    #endregion

    #region Language

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [BindToSetting(Path = nameof(SettingsService.CurrentLanguage))]
    public partial string CurrentLanguage { get; set; }

    partial void OnCurrentLanguageChanged(string oldValue, string newValue)
    {
        if (ResourceUtils.Languages.Contains(CurrentLanguage) && oldValue is not null) // oldValue is null at startup
            ResourceUtils.ApplyLanguage(CurrentLanguage);
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
        var folderPicker = new FolderPicker();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
        {
            if (MinecraftFolders.Contains(folder.Path))
            {
                _notificationService.NotifyMessage(
                    ResourceUtils.GetValue("Notifications", "_AddFolderExistedT"),
                    ResourceUtils.GetValue("Notifications", "_AddFolderExistedD"),
                    icon: "\uF89A");

                return;
            }

            _gameService.AddMinecraftFolder(folder.Path);
        }
    }

    private readonly string OfficialLauncherPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\.minecraft";

    [RelayCommand]
    public void DetectOfficialMinecraftFolder()
    {
        // Official launcher .minecraft folder not exist
        if (!Directory.Exists(OfficialLauncherPath))
        {
            _notificationService.NotifyMessage(
                ResourceUtils.GetValue("Notifications", "_AddOfficialFolderNotExistT"),
                ResourceUtils.GetValue("Notifications", "_AddOfficialFolderNotExistD").Replace("${path}", OfficialLauncherPath),
                icon: "\uF89A");

            return;
        }

        // Already added
        if (MinecraftFolders.Contains(OfficialLauncherPath))
        {
            _notificationService.NotifyMessage(
                ResourceUtils.GetValue("Notifications", "_AddOfficiaFolderExistedT"),
                ResourceUtils.GetValue("Notifications", "_AddOfficiaFolderExistedD").Replace("${path}", OfficialLauncherPath),
                icon: "\uF89A");

            return;
        }

        // Add to list

        _gameService.AddMinecraftFolder(OfficialLauncherPath);

        _notificationService.NotifyMessage(
            ResourceUtils.GetValue("Notifications", "_AddOfficiaFolderAddedT"),
            ResourceUtils.GetValue("Notifications", "_AddOfficiaFolderAddedD").Replace("${path}", OfficialLauncherPath),
            icon: "\uE73E");
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
                _notificationService.NotifyMessage(
                    ResourceUtils.GetValue("Notifications", "_AddJavaExistedT"),
                    ResourceUtils.GetValue("Notifications", "_AddJavaExistedD"),
                    icon: "\uF89A");

                return;
            }

            JavaRuntimes.Add(openFileDialog.FileName); 
            OnPropertyChanged(nameof(JavaRuntimes));

            ActiveJavaRuntime = openFileDialog.FileName;
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

            _notificationService.NotifyWithoutContent(
                ResourceUtils.GetValue("Notifications", "_AddSearchedJavaT"),
                icon: "\uE73E");
        }
        catch (Exception ex)
        {
            _notificationService.NotifyException("_AddSearchedJavaFailedT", ex);
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
                return ResourceUtils.GetValue("OOBE_OOBEViewModel__ButtonGetStarted");
            else
                return ResourceUtils.GetValue("OOBE_OOBEViewModel__ButtonNext");
        }
    }

    [RelayCommand]
    public void Start()
    {
        _navigationService.Parent?.NavigateTo("ShellPage");
        (App.MainWindow.MinWidth, App.MainWindow.MinHeight) = (516, 328);

        _settings.FinishGuide = true;
    }

}
