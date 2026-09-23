using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.DataPacks;

internal partial class NavigationViewModel(INavigationService navigationService)
    : NavigationPageVM(navigationService), INavigationAware
{
    protected override string RootPageKey => "DataPacksDownload";

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("DataPacksDownload/Default", parameter);
}
