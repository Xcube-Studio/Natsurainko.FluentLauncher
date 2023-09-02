using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

#nullable enable

class SettingsNavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public SettingsNavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is string pageKey)
            _navigationService.NavigateTo(pageKey);
        else
            _navigationService.NavigateTo("LaunchSettingsPage"); // Default page
    }

    public void NavigateTo(string pageKey, object? parameter = null)
        => _navigationService.NavigateTo(pageKey, parameter);
}
