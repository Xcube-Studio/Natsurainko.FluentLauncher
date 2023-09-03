using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

#nullable enable

class ActivitiesNavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public ActivitiesNavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    #region Navigation

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is string pageKey)
            _navigationService.NavigateTo(pageKey);
        else
            _navigationService.NavigateTo("LaunchTasksPage");
    }

    public void NavigateTo(string pageKey, object? parameter = null)
        => _navigationService.NavigateTo(pageKey, parameter);

    #endregion
}
