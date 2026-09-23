using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Worlds;

internal partial class NavigationViewModel(INavigationService navigationService)
    : NavigationPageVM(navigationService), INavigationAware
{
    protected override string RootPageKey => "WorldsDownload";

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("WorldsDownload/Default", parameter);
}
