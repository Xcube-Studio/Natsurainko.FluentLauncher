using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Management;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;
    private readonly GameService _gameService;

    public GameInfo GameInfo { get; private set; }

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
        GameInfo = parameter as GameInfo;
        GameConfig = GameInfo.GetConfig();

        Task.Run(() =>
        {
            var gameStorageInfo = GameInfo.GetStorageInfo();
            App.DispatcherQueue.TryEnqueue(() => GameStorageInfo = gameStorageInfo);
        });

        GameConfig.PropertyChanged += GameConfig_PropertyChanged;
    }

    private void GameConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NickName" && !string.IsNullOrEmpty(GameConfig.NickName))
        {
            if (GameInfo.Equals(_gameService.ActiveGame))
                _gameService.ActiveGame.Name = GameConfig.NickName;

            GameInfo.Name = GameConfig.NickName;
        }
    }

    [RelayCommand]
    void CardClick(string tag) => _navigationService.NavigateTo(tag, GameInfo);

    [RelayCommand]
    public async Task DeleteGame() => await new DeleteGameDialog()
    {
        DataContext = new DeleteGameDialogViewModel(GameInfo, _navigationService)
    }.ShowAsync();
}
