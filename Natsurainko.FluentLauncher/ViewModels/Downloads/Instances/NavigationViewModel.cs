using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.Downloads.Instances;

internal partial class NavigationViewModel : NavigationPageVM, INavigationAware
{
    protected override string RootPageKey => "InstancesDownload";

    public NavigationViewModel(INavigationService navigationService)
        : base(navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter) => NavigateTo("InstancesDownload/Default", parameter); // Default page
}
