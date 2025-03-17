using FluentLauncher.Infra.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels.News;

internal partial class NavigationViewModel : NavigationPageVM, INavigationAware
{
    protected override string RootPageKey => "News";

    public NavigationViewModel(INavigationService navigationService)
        : base(navigationService)
    {
        NavigationService = navigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
            NavigateTo(pageKey);
        else
            NavigateTo("News/Default"); // Default page
    }
}
