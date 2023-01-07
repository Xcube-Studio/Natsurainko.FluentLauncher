using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Model.Launch;
using Natsurainko.FluentCore.Module.Launcher;
using Natsurainko.FluentLauncher.Components.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Pages;

public partial class Cores : ObservableObject
{
    public Cores() 
    {
        GameFolders = new(App.Configuration.GameFolders);
        CurrentGameFolder = App.Configuration.CurrentGameFolder;
    }

    [ObservableProperty]
    private ObservableCollection<GameCore> gameCores;

    [ObservableProperty]
    private GameCore currentGameCore;

    [ObservableProperty]
    private ObservableCollection<string> gameFolders;

    [ObservableProperty]
    private string currentGameFolder;

    [RelayCommand]
    private void Loaded(object parameter)
        => parameter.As<ListView, object>(e => e.sender.ScrollIntoView(e.sender.SelectedItem));

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(CurrentGameCore))
            App.Configuration.CurrentGameCore = CurrentGameCore?.Id;

        if (e.PropertyName == nameof(CurrentGameFolder) && !string.IsNullOrEmpty(App.Configuration.CurrentGameFolder))
        {
            App.Configuration.CurrentGameFolder = CurrentGameFolder;

            Task.Run(() =>
            {
                var cores = new GameCoreLocator(App.Configuration.CurrentGameFolder).GetGameCores();
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    GameCores = new(cores);
                    CurrentGameCore = GameCores.Where(x => x.Id == App.Configuration.CurrentGameCore).FirstOrDefault();
                });
            });
        }
    }
}
