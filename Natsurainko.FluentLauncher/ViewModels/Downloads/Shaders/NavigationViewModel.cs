using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Shaders;

internal partial class NavigationViewModel(INavigationService navigationService)
    : NavigationPageVM(navigationService), INavigationAware
{
    protected override string RootPageKey => "ShadersDownload";

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("ShadersDownload/Default", parameter);
}
