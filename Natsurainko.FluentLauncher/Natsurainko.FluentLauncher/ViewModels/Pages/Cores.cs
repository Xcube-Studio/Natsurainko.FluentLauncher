using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

public partial class Cores : ObservableObject
{
    public Cores()
    {
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

            var cores = new GameCoreLocator(App.Configuration.CurrentGameFolder).GetGameCores();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                GameCores = new(cores);
                CurrentGameCore = GameCores.Where(x => x.Id == App.Configuration.CurrentGameCore).FirstOrDefault();

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

        if (e.PropertyName == nameof(CurrentGameCore))
            App.Configuration.CurrentGameCore = CurrentGameCore?.Id;

        if (e.PropertyName == nameof(CurrentGameFolder))
        {
            App.Configuration.CurrentGameFolder = CurrentGameFolder;
            RefreshCores();
        }

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
    private ObservableCollection<GameCore> gameCores;

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
    private Visibility tipVisibility = Visibility.Collapsed;
}
