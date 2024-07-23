using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Nrk.FluentCore.Management;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class DefaultViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public GameInfo GameInfo { get; private set; }

    public DefaultViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    [RelayCommand]
    void CardClick(string tag) => _navigationService.NavigateTo(tag, GameInfo);

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        GameInfo = parameter as GameInfo;
    }
}
