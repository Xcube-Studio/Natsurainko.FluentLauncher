using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.FluentCore;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Views.Pages;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

public partial class Cores : ObservableObject
{
    public Cores()
    {
        filter = App.Configuration.CoresFilter;
        sortBy = App.Configuration.CoresSortBy;

        GameFolders = new(App.Configuration.GameFolders);
        CurrentGameFolder = App.Configuration.CurrentGameFolder;
    }

    private ListView ListView;

    private bool EnableCoreCommand()
        => CurrentGameCore != null;

    private bool EnableFolderCommand()
        => CurrentGameFolder != null;

    private void RefreshCores()
    {
        Task.Run(() =>
        {
            if (string.IsNullOrEmpty(CurrentGameFolder))
                return;

            seletedChangeable = false;
            var cores = FilterAndSortGameCores(new GameCoreLocator(App.Configuration.CurrentGameFolder).GetGameCores());

            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                GameCores = new(cores);

                seletedChangeable = true;
                var current = GameCores.Where(x => x.Id == App.Configuration.CurrentGameCore).FirstOrDefault();

                if (current == CurrentGameCore)
                    OnPropertyChanged(nameof(CurrentGameCore));
                else CurrentGameCore = current;

                if (ListView != null)
                    ListView.ScrollIntoView(ListView.SelectedItem);

                if (!GameCores.Any())
                {
                    TipVisibility = Visibility.Visible;
                    TipTitle = "No Game Cores";
                    TipSubTitle = "Go to Resources to install";
                }
                else TipVisibility = Visibility.Collapsed;
            });
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(CurrentGameCore) && seletedChangeable)
            App.Configuration.CurrentGameCore = CurrentGameCore?.Id;

        if (e.PropertyName == nameof(CurrentGameFolder) 
            || e.PropertyName == nameof(Filter)
            || e.PropertyName == nameof(SortBy)
            || e.PropertyName == nameof(Search))
        {
            App.Configuration.CurrentGameFolder = CurrentGameFolder;
            RefreshCores();
        }

        if (e.PropertyName == nameof(Filter))
            App.Configuration.CoresFilter = Filter;

        if (e.PropertyName == nameof(SortBy))
            App.Configuration.CoresSortBy = SortBy;

        if (e.PropertyName == nameof(GameFolders))
        {
            if (string.IsNullOrEmpty(App.Configuration.CurrentGameFolder))
            {
                TipVisibility = Visibility.Visible;
                TipTitle = "No Game Folders";
                TipSubTitle = "Go to Settings>Global Launch Settings to add";

                return;
            }
            else TipVisibility = Visibility.Collapsed;
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
}

public partial class Cores
{
    [RelayCommand]
    private void GoToPage()
    {
        if (TipTitle.Equals("No Game Cores"))
            MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Resources.Navigation));
        else if (TipTitle.Equals("No Game Folders"))
            MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Settings.Navigation));
    }

    [RelayCommand]
    private void Loaded(object parameter)
        => parameter.As<ListView, object>(e =>
        {
            ListView = e.sender;
            e.sender.ScrollIntoView(e.sender.SelectedItem);
        });

    [RelayCommand(CanExecute = nameof(EnableCoreCommand))]
    private Task Launch() => Task.Run(() => LaunchArrangement.StartNew(CurrentGameCore));

    [RelayCommand(CanExecute = nameof(EnableCoreCommand))]
    private void Delete()
    {

    }

    [RelayCommand(CanExecute = nameof(EnableCoreCommand))]
    private void Rename()
    {

    }

    [RelayCommand(CanExecute = nameof(EnableCoreCommand))]
    private void GenerateScript()
    {

    }

    [RelayCommand(CanExecute = nameof(EnableFolderCommand))]
    private Task OpenFolder() 
        => Task.Run(async () => await Launcher.LaunchFolderPathAsync(CurrentGameFolder));

    [RelayCommand(CanExecute = nameof(EnableFolderCommand))]
    private void InstallCore()
        => MainContainer.ContentFrame.Navigate(typeof(Views.Pages.Resources.Navigation));
}

public partial class Cores
{
    [ObservableProperty]
    private ObservableCollection<GameCore> gameCores = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LaunchCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyCanExecuteChangedFor(nameof(RenameCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateScriptCommand))]
    private GameCore currentGameCore;

    [ObservableProperty]
    private ObservableCollection<string> gameFolders;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenFolderCommand))]
    [NotifyCanExecuteChangedFor(nameof(InstallCoreCommand))]
    private string currentGameFolder;

    [ObservableProperty]
    private string tipTitle;

    [ObservableProperty]
    private string tipSubTitle;

    [ObservableProperty]
    private string filter;

    [ObservableProperty]
    private string sortBy;

    [ObservableProperty]
    private string search;

    [ObservableProperty]
    private Visibility tipVisibility = Visibility.Collapsed;
}
