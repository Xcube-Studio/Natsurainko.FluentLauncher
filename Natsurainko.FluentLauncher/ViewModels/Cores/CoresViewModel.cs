using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Extension;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Views.Cores;
using Natsurainko.FluentLauncher.Views.Cores.InstallNewCore;
using Natsurainko.FluentLauncher.Views.Cores.Properties;
using Natsurainko.FluentLauncher.Views.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.System;
using GameCoreLocator = Natsurainko.FluentLauncher.Components.FluentCore.GameCoreLocator;
using Natsurainko.FluentLauncher.Services.Settings;
using AppSettingsManagement.Mvvm;
using Natsurainko.FluentLauncher.ViewModels.Common;
using System.Collections.Specialized;

namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class CoresViewModel : SettingsViewModelBase, ISettingsViewModel
{
    private static readonly Regex NameRegex = new("^[^/\\\\:\\*\\?\\<\\>\\|\"]{1,255}$");

    #region Settings

    [SettingsProvider]
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresFilter))]
    private string filter;

    [ObservableProperty]
    [BindToSetting(Path = nameof(SettingsService.CoresSortBy))]
    private string sortBy;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenInstallCommand))]
    [BindToSetting(Path = nameof(SettingsService.CurrentGameFolder))]
    private string currentGameFolder;

    [BindToSetting(Path = nameof(SettingsService.GameFolders))]
    public ObservableCollection<string> GameFolders { get; private set; } = null!;

    #endregion

    #region ObservableProperty

    public ObservableCollection<GameCore> GameCores { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenDeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenRenameCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateScriptCommand))]
    private GameCore currentGameCore;

    [ObservableProperty]
    private string tipTitle;

    [ObservableProperty]
    private string tipSubTitle;

    [ObservableProperty]
    private string search;

    [ObservableProperty]
    private Visibility tipVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private bool renameIsOpen;

    [ObservableProperty]
    private bool deleteIsOpen;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
    private string newName;

    [ObservableProperty]
    private GameCore toRename;

    [ObservableProperty]
    private GameCore toDelete;

    #endregion

    public CoresViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        (this as ISettingsViewModel).InitializeSettings();
        RefreshCores();
        PropertyChanged += CoresViewModel_PropertyChanged;
        GameFolders.CollectionChanged += GameFolders_CollectionChanged;
    }

    private void GameFolders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(_settingsService.CurrentGameFolder))
        {
            TipVisibility = Visibility.Visible;
            TipTitle = "No Game Folders";
            TipSubTitle = "Go to Settings>Global Launch Settings to add";

            return;
        }
        else TipVisibility = Visibility.Collapsed;
    }

    private ListView ListView;

    private bool EnableFolderCommand()
        => !string.IsNullOrEmpty(CurrentGameFolder);

    private bool EnableRenameCommand()
        => !string.IsNullOrEmpty(NewName) && NameRegex.Matches(NewName).Any() && !GameCores.Where(x => x.Id.Equals(NewName)).Any();

    private void RefreshCores()
    {
        Task.Run(() =>
        {
            if (string.IsNullOrEmpty(CurrentGameFolder))
                return;

            seletedChangeable = false;
            var cores = FilterAndSortGameCores(new GameCoreLocator(_settingsService.CurrentGameFolder).GetGameCores());

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                GameCores.Clear();
                foreach (var item in cores)
                {
                    GameCores.Add(item);
                }

                seletedChangeable = true;
                var current = GameCores.Where(x => x.Id == _settingsService.CurrentGameCore).FirstOrDefault(cores.FirstOrDefault());

                if (current == CurrentGameCore)
                    OnPropertyChanged(nameof(CurrentGameCore));
                else CurrentGameCore = current;

                if (ListView != null)
                    ListView.ScrollIntoView(ListView.SelectedItem);

                if (!GameCores.Any())
                {
                    TipVisibility = Visibility.Visible;
                    TipTitle = "No Game Cores";
                    TipSubTitle = "Click \"Install New Core\" to install";
                }
                else TipVisibility = Visibility.Collapsed;
            });
        });
    }

    private void CoresViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentGameCore) && seletedChangeable)
            _settingsService.CurrentGameCore = CurrentGameCore?.Id;

        if (e.PropertyName == nameof(Filter) ||
            e.PropertyName == nameof(SortBy) ||
            e.PropertyName == nameof(Search))
        {
            RefreshCores();
        }
    }

    private IEnumerable<GameCore> FilterAndSortGameCores(IEnumerable<GameCore> cores)
    {
        IEnumerable<GameCore> filtered = default;

        if (Filter.Equals("All"))
            filtered = cores;
        else if (Filter.Equals("Release"))
            filtered = cores.Where(x => x.Type.Equals("release"));
        else if (Filter.Equals("Snapshot"))
            filtered = cores.Where(x => x.Type.Equals("snapshot"));
        else if (Filter.Equals("Old"))
            filtered = cores.Where(x => x.Type.Contains("old_"));

        IEnumerable<GameCore> list = default;

        list = SortBy == "Launch Date"
            ? filtered.OrderByDescending(x => x.CoreProfile.LastLaunchTime.GetValueOrDefault())
            : filtered.OrderBy(x => x.Id);

        if (Search != null)
            list = list.Where(x => x.Id.ToLower().Contains(Search.ToLower()));

        return list;
    }

    private bool seletedChangeable = true;

    [RelayCommand]
    private void GoToPage()
    {
        if (TipTitle.Equals("No Game Cores"))
            OpenInstall();
        else if (TipTitle.Equals("No Game Folders"))
            Views.ShellPage.ContentFrame.Navigate(typeof(Views.Settings.NavigationPage));
    }

    [RelayCommand]
    private void Loaded(object parameter)
        => parameter.As<ListView, object>(e =>
        {
            ListView = e.sender;
            e.sender.ScrollIntoView(e.sender.SelectedItem);
        });

    [RelayCommand]
    private Task Launch(GameCore core) => Task.Run(() => LaunchArrangement.StartNew(core));

    [RelayCommand]
    private void OpenDelete(GameCore data)
    {
        if (data.Equals(ToDelete))
            OnPropertyChanged(nameof(ToDelete));

        ToDelete = data;
        DeleteIsOpen = true;
    }

    [RelayCommand]
    private void OpenRename(GameCore data)
    {
        if (data.Equals(ToRename))
            OnPropertyChanged(nameof(ToRename));

        ToRename = data;
        RenameIsOpen = true;
    }

    [RelayCommand]
    private void CancelRename()
    {
        NewName = null;
        RenameIsOpen = false;
    }

    [RelayCommand]
    private Task OpenFolder(GameCore core) => Task.Run(async () => await Launcher.LaunchFolderPathAsync(core.Root.FullName));

    [RelayCommand]
    private void OpenOptions(GameCore core)
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            var coreOptionsDialog = new CorePropertiesDialog
            {
                XamlRoot = Views.ShellPage._XamlRoot,
                DataContext = core
            };
            await coreOptionsDialog.ShowAsync();
        });
    }

    [RelayCommand(CanExecute = nameof(EnableFolderCommand))]
    private void OpenInstall()
    {
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            var installCoreDialog = new InstallCoreDialog
            {
                XamlRoot = Views.ShellPage._XamlRoot,
                InstalledCoreNames = GameCores.Select(x => x.Id).ToArray()
            };
            await installCoreDialog.ShowAsync();
        });
    }

    [RelayCommand]
    private void Delete()
    {
        if (File.Exists(ToDelete.CoreProfile.FilePath))
            File.Delete(ToDelete.CoreProfile.FilePath);

        ToDelete.Delete();

        if (ToDelete.Equals(CurrentGameCore))
        {
            CurrentGameCore = null;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentGameCore)));
        }

        RefreshCores();

        DeleteIsOpen = false;
        ToDelete = null;
    }

    [RelayCommand(CanExecute = nameof(EnableRenameCommand))]
    private void Rename()
    {
        ToRename.Rename(NewName);

        if (File.Exists(ToRename.CoreProfile.FilePath))
        {
            File.Delete(ToRename.CoreProfile.FilePath);

            ToRename.CoreProfile.FilePath = ToRename.GetFileOfProfile().FullName;
            ToRename.CoreProfile.Id = NewName;
        }

        if (ToRename.Equals(CurrentGameCore))
            _settingsService.CurrentGameCore = NewName;

        RefreshCores();

        RenameIsOpen = false;
        NewName = null;
        ToRename = null;
    }

    [RelayCommand]
    private Task GenerateScript(GameCore core) => Task.Run(async () =>
    {
        var savePicker = new FileSavePicker();

        savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
        savePicker.FileTypeChoices.Add("Batch File", new List<string>() { ".bat" });
        savePicker.SuggestedFileName = $"{core.Id}-LaunchScript";

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

        var file = await savePicker.PickSaveFileAsync();

        if (file != null)
        {
            await File.WriteAllTextAsync(file.Path, core.MakeLaunchScript());

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                var hyperlinkButton = new HyperlinkButton
                {
                    Content = new TextBlock
                    {
                        Text = file.Path,
                        TextTrimming = TextTrimming.CharacterEllipsis
                    }
                };

                hyperlinkButton.Click += (_, e) => _ = Launcher.LaunchFolderPathAsync(new FileInfo(file.Path).Directory.FullName);

                MessageService.Show(
                    $"Succeffully Generate Launch Script for {core.Id}",
                    severity: InfoBarSeverity.Success,
                    button: hyperlinkButton);
            });
        }
        else MessageService.Show($"Cancelled Generate Launch Script for {core.Id}");
    });
}
