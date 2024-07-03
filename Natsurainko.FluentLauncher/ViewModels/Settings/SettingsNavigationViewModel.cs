using CommunityToolkit.Mvvm.ComponentModel;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Services.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

#nullable enable

class SettingsNavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public SettingsNavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
            _navigationService.NavigateTo(pageKey);
        else
            _navigationService.NavigateTo("LaunchSettingsPage"); // Default page
    }

    public void NavigateTo(string pageKey, object? parameter = null)
        => _navigationService.NavigateTo(pageKey, parameter);
}
