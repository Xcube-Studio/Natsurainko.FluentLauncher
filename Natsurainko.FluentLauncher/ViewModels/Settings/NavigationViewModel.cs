using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Settings;

internal partial class NavigationViewModel(INavigationService navigationService)
    : NavigationPageVM(navigationService), INavigationAware
{
    protected override string RootPageKey => "Settings";

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
            NavigateTo(pageKey);
        else
            NavigateTo("Settings/Default"); // Default page
    }
}
