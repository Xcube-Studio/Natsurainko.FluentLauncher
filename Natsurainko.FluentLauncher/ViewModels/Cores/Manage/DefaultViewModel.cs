using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Management;
using System.ComponentModel;
using System.Threading.Tasks;
using System;
using Nrk.FluentCore.GameManagement;
using Nrk.FluentCore.GameManagement.Instances;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;

    public MinecraftInstance MinecraftInstance { get; private set; }

    public GameConfig GameConfig { get; private set; }

    public DefaultViewModel(GameService gameService, INavigationService navigationService)
    {
        _gameService = gameService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormatSize))]
    private GameStorageInfo gameStorageInfo;

    public string FormatSize
    {
        get
        {
            if (GameStorageInfo == null)
                return string.Empty;

            double d = GameStorageInfo.TotalSize;
            int i = 0;

            while ((d > 1024) && (i < 5))
            {
                d /= 1024;
                i++;
            }

            var unit = new string[] { "B", "KB", "MB", "GB", "TB" };
            return string.Format("{0} {1}", Math.Round(d, 2), unit[i]);
        }
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        GameConfig = MinecraftInstance.GetConfig();

        Task.Run(() =>
        {
            var gameStorageInfo = MinecraftInstance.GetStatistics();
            App.DispatcherQueue.TryEnqueue(() => GameStorageInfo = gameStorageInfo);
        });

        GameConfig.PropertyChanged += GameConfig_PropertyChanged;
    }

    private void GameConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NickName" && !string.IsNullOrEmpty(GameConfig.NickName))
        {
            if (MinecraftInstance.Equals(_gameService.ActiveGame))
                _gameService.ActiveGame.GetConfig().NickName = GameConfig.NickName;

            MinecraftInstance.GetConfig().NickName = GameConfig.NickName;
        }
    }

    [RelayCommand]
    void CardClick(string tag) => _navigationService.NavigateTo(tag, MinecraftInstance);

    [RelayCommand]
    public async Task DeleteGame() => await new DeleteGameDialog()
    {
        DataContext = new DeleteGameDialogViewModel(MinecraftInstance, _navigationService)
    }.ShowAsync();
}
