using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Modpacks;

internal partial class NavigationViewModel(INavigationService navigationService)
    : NavigationPageVM(navigationService), INavigationAware
{
    protected override string RootPageKey => "ModpacksDownload";

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("ModpacksDownload/Default", parameter);
}
