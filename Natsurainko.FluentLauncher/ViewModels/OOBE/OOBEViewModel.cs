using AppSettingsManagement.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using Nrk.FluentCore.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace Natsurainko.FluentLauncher.ViewModels.OOBE;

internal partial class OOBEViewModel : ObservableRecipient, INavigationAware, ISettingsViewModel, IRecipient<ActiveAccountChangedMessage>
{
    #region Dependencies

    [SettingsProvider]
    private readonly SettingsService _settings;

    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;
    private readonly NotificationService _notificationService;
    private readonly AccountService _accountService;

    #endregion

    public OOBEViewModel(
        INavigationService navigationService,
        SettingsService settings,
        GameService gameService,
        NotificationService notificationService,
        AccountService accountService)
    {
        _navigationService = navigationService;
        _settings = settings;
        _gameService = gameService;
        _notificationService = notificationService;
        _accountService = accountService;

        // Init accounts
        Accounts = accountService.Accounts;
        ActiveAccount = accountService.ActiveAccount;
        IsActive = true; // Enable message recipient

        (this as ISettingsViewModel).InitializeSettings();
    }

    #region Navigation

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    int currentPageIndex;

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
        CurrentPageIndex++;
        _navigationService.NavigateTo(OOBEPageKeys[CurrentPageIndex]);
        BackCommand.NotifyCanExecuteChanged();
        NextCommand.NotifyCanExecuteChanged();
    }

    bool CanNext() => CurrentPageIndex switch
    {
        // Language page
        0 => Languages.Contains(CurrentLanguage),
        // Minecraft folder page
        1 => !string.IsNullOrEmpty(ActiveMinecraftFolder) &&
                                    !string.IsNullOrEmpty(ActiveJava) &&
                                    Directory.Exists(ActiveMinecraftFolder) &&
                                    File.Exists(ActiveJava),
        // Java page
        2 => true, // TODO:
        // Account page
        3 => ActiveAccount is not null,
        // Get started page
        4 => false,
        // Default
        _ => false,
    };

    [RelayCommand(CanExecute = nameof(CanBack))]
    public void Back()
    {
        _navigationService.GoBack();
        CurrentPageIndex--;
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

    #endregion

    #region Language

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [BindToSetting(Path = nameof(SettingsService.CurrentLanguage))]
    private string currentLanguage;

    public List<string> Languages { get; } = ResourceUtils.Languages;

    partial void OnCurrentLanguageChanged(string oldValue, string newValue)
    {
        if (Languages.Contains(CurrentLanguage) && oldValue is not null) // oldValue is null at startup
            ResourceUtils.ApplyLanguage(CurrentLanguage);
    }

    #endregion

    #region Minecraft folder

    [BindToSetting(Path = nameof(SettingsService.MinecraftFolders))]
    public ObservableCollection<string> MinecraftFolders { get; private set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [BindToSetting(Path = nameof(SettingsService.ActiveMinecraftFolder))]
    private string activeMinecraftFolder;

    [RelayCommand]
    public Task BrowserFolder() => Task.Run(async () =>
    {
        var folderPicker = new FolderPicker();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        folderPicker.FileTypeFilter.Add("*");
        var folder = await folderPicker.PickSingleFolderAsync();

        if (folder != null)
            App.DispatcherQueue.TryEnqueue(() =>
            {
                if (MinecraftFolders.Contains(folder.Path))
                {
                    _notificationService.NotifyMessage("Failed to add the folder", "This folder already exists", icon: "\uF89A");
                    return;
                }

                MinecraftFolders.Add(folder.Path);
                _gameService.ActivateMinecraftFolder(folder.Path);
            });
    });

    #endregion

    #region Java

    [BindToSetting(Path = nameof(SettingsService.Javas))]
    public ObservableCollection<string> Javas { get; private set; }

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.ActiveJava))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string activeJava;

    [ObservableProperty]
    private bool javaDropDownOpen;

    [RelayCommand]
    public Task BrowserJava() => Task.Run(async () =>
    {
        var filePicker = new FileOpenPicker();

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

        filePicker.FileTypeFilter.Add(".exe");
        var file = await filePicker.PickSingleFileAsync();

        if (file != null)
            App.DispatcherQueue.TryEnqueue(() =>
            {
                Javas.Add(file.Path);
                OnPropertyChanged(nameof(Javas));

                ActiveJava = file.Path;
            });
    });

    [RelayCommand]
    public void SearchJava()
    {
        foreach (var java in JavaUtils.SearchJava())
            if (!Javas.Contains(java))
                Javas.Add(java);

        ActiveJava = Javas.Any() ? Javas[0] : null;

        OnPropertyChanged(nameof(Javas));

        JavaDropDownOpen = true;
        _notificationService.NotifyWithoutContent("Added the search Java to the runtime list", icon: "\uE73E");
    }

    #endregion

    #region Account

    public ReadOnlyObservableCollection<Account> Accounts { get; init; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private Account activeAccount;

    bool processingActiveAccountChangedMessage = false;

    public void Receive(ActiveAccountChangedMessage message)
    {
        processingActiveAccountChangedMessage = true;
        ActiveAccount = message.Value;
        processingActiveAccountChangedMessage = false;
    }

    partial void OnActiveAccountChanged(Account value)
    {
        if (!processingActiveAccountChangedMessage)
            _accountService.Activate(value);
    }

    [RelayCommand]
    public void Login(Button parameter)
        => _ = new AuthenticationWizardDialog { XamlRoot = parameter.XamlRoot }.ShowAsync();

    #endregion

    [RelayCommand]
    public void Start()
    {
        _navigationService.Parent?.NavigateTo("ShellPage");
        _settings.FinishGuide = true;
    }

}
