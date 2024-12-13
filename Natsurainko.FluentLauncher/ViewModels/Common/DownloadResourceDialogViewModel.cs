using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Services.Network;
using Natsurainko.FluentLauncher.Services.Network.Data;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Common;

internal partial class DownloadResourceDialogViewModel : ObservableObject
{
    private ContentDialog _dialog;
    private GameResourceFile[] ResourceFileItems;

    private readonly object _resource;
    private readonly INavigationService _navigationService;

    private readonly CurseForgeClient _curseForgeClient = App.GetService<CurseForgeClient>();
    private readonly ModrinthClient _modrinthClient = App.GetService<ModrinthClient>();

    private readonly GameService _gameService = App.GetService<GameService>();
    private readonly DownloadService _downloadService = App.GetService<DownloadService>();
    private readonly NotificationService _notificationService = App.GetService<NotificationService>();

    public MinecraftInstance MinecraftInstance { get; private set; }

    public DownloadResourceDialogViewModel(object resource, INavigationService navigationService)
    {
        _resource = resource;
        _navigationService = navigationService;

        MinecraftInstance = _gameService.ActiveGame;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(SelectedLoader)
            || e.PropertyName == nameof(SelectedVersion))
            FilterFiles();

        if (e.PropertyName == nameof(DownloadToCurrentGame)
            || e.PropertyName == nameof(DownloadToDesignated)
            || e.PropertyName == nameof(IsModpack))
        {
            if (DownloadToCurrentGame) DownloadToDesignated = IsModpack = false;
            if (DownloadToDesignated) DownloadToCurrentGame = IsModpack = false;
            if (IsModpack) DownloadToDesignated = DownloadToCurrentGame = false;

            if (DownloadToCurrentGame || DownloadToDesignated || IsModpack)
            {
                CheckBox1 = DownloadToCurrentGame ? Visibility.Visible : Visibility.Collapsed;
                CheckBox2 = DownloadToDesignated ? Visibility.Visible : Visibility.Collapsed;
                CheckBox3 = IsModpack ? Visibility.Visible : Visibility.Collapsed;
            }
            else CheckBox1 = CheckBox2 = CheckBox3 = Visibility.Visible;
        }
    }

    [ObservableProperty]
    public partial string[] Versions { get; set; }

    [ObservableProperty]
    public partial string SelectedVersion { get; set; }

    [ObservableProperty]
    public partial string[] Loaders { get; set; }

    [ObservableProperty]
    public partial string SelectedLoader { get; set; }

    [ObservableProperty]
    public partial string ResourceName { get; set; }

    [ObservableProperty]
    public partial GameResourceFile[] DisplayItems { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial GameResourceFile SelectedItem { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial bool DownloadToDesignated { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial bool DownloadToCurrentGame { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial bool IsModpack { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    public partial string DesignatedFilePath { get; set; }

    [ObservableProperty]
    public partial Visibility CheckBox1 { get; set; }

    [ObservableProperty]
    public partial Visibility CheckBox2 { get; set; }

    [ObservableProperty]
    public partial Visibility CheckBox3 { get; set; }

    public bool CanConfirm
    {
        get
        {
            if (SelectedItem == null
                || !(DownloadToCurrentGame || DownloadToDesignated || IsModpack)
                || (DownloadToDesignated && string.IsNullOrEmpty(DesignatedFilePath)))
                return false;

            return true;
        }
    }

    [RelayCommand]
    public void LoadEvent(object args)
    {
        var grid = args.As<Grid, object>().sender;
        _dialog = grid.FindName("Dialog") as ContentDialog;

        Task.Run(ParseResource);
    }

    [RelayCommand]
    public void Cancel() => _dialog.Hide();

    [RelayCommand]
    public void Confirm()
    {
        string fileName = string.Empty;

        if (DownloadToDesignated) fileName = DesignatedFilePath;
        else if (DownloadToCurrentGame) fileName = Path.Combine(MinecraftInstance.GetModsDirectory(), SelectedItem.FileName);

        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException(nameof(fileName));

        _downloadService.DownloadResourceFile(SelectedItem, fileName);
        _dialog.Hide();

        _notificationService.NotifyWithoutContent(ResourceUtils.GetValue("Notifications", "_AddDownloadTask"), icon: "\ue896");
    }

    public void SaveFile()
    {
        var saveFileDialog = new SaveFileDialog
        {
            FileName = SelectedItem == null ? string.Empty : SelectedItem.FileName,
            InitialDirectory = _gameService.ActiveMinecraftFolder
        };

        if (saveFileDialog.ShowDialog().GetValueOrDefault())
            DesignatedFilePath = saveFileDialog.FileName;
    }

    async Task ParseResource()
    {
        string resourceName = string.Empty;

        var fileItems = new List<GameResourceFile>();
        var loaders = new List<string>();
        var versions = new List<string>();

        if (_resource is CurseForgeResource curseForgeResource)
        {
            resourceName = curseForgeResource.Name;

            foreach (var x in curseForgeResource.Files)
            {
                var loader = x.ModLoaderType.ToString();

                if (!loaders.Contains(loader))
                    loaders.Add(loader);

                if (!versions.Contains(x.McVersion))
                    versions.Add(x.McVersion);

                fileItems.Add(new GameResourceFile(_curseForgeClient.GetFileUrlAsync(x))
                {
                    FileName = x.FileName,
                    Version = x.McVersion,
                    Loaders = [loader]
                });
            }
        }
        else if (_resource is ModrinthResource modrinthResource)
        {
            resourceName = modrinthResource.Name;

            foreach (var x in await _modrinthClient.GetProjectVersionsAsync(modrinthResource.Id))
            {
                foreach (var loader in x.Loaders)
                    if (!loaders.Contains(loader))
                        loaders.Add(loader);

                if (!versions.Contains(x.McVersion))
                    versions.Add(x.McVersion);

                fileItems.Add(new GameResourceFile(Task.FromResult(x.Url))
                {
                    FileName = x.FileName,
                    Version = x.McVersion,
                    Loaders = x.Loaders
                });
            }
        }

        ResourceFileItems = [.. fileItems];

        App.DispatcherQueue.TryEnqueue(() =>
        {
            ResourceName = resourceName;
            Versions = [.. versions];
            Loaders = [.. loaders];

            SelectedVersion = Versions.First();
            SelectedLoader = Loaders.First();

            FilterFiles();
        });
    }

    void FilterFiles() => DisplayItems = [.. ResourceFileItems.Where(x => x.Version == SelectedVersion && (x.Loaders.Contains("Any") || x.Loaders.Contains(SelectedLoader)))];
}