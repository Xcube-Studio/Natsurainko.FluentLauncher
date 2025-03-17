using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

internal partial class NavigationViewModel(INavigationService navigationService) 
    : NavigationPageVM(navigationService), INavigationAware
{
    protected override string RootPageKey => "ModsDownload";

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("ModsDownload/Default", parameter);
}
