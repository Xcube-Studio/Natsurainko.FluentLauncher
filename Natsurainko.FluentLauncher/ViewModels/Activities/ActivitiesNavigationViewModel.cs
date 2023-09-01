using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.UI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Activities;

class ActivitiesNavigationViewModel : ObservableObject, INavigationAware
{
    private readonly INavigationService _navigationService;

    public ActivitiesNavigationViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is string pageKey)
            _navigationService.NavigateTo(pageKey);
    }
}
