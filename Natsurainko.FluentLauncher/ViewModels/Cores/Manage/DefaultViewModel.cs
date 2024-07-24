using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Management;
using System.ComponentModel;

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

    [RelayCommand]
    void CardClick(string tag) => _navigationService.NavigateTo(tag, GameInfo);

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        GameInfo = parameter as GameInfo;
        GameConfig = GameInfo.GetConfig();

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
}
