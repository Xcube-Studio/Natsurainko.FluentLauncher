using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.TexturePacks;

internal partial class NavigationViewModel(INavigationService navigationService)
    : NavigationPageVM(navigationService), INavigationAware
{
    protected override string RootPageKey => "TexturePacksDownload";

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("TexturePacksDownload/Default", parameter);
}
