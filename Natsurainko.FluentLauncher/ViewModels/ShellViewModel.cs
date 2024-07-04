using FluentLauncher.Infra.UI.Navigation;


namespace Natsurainko.FluentLauncher.ViewModels;

class ShellViewModel : INavigationAware
{
    public INavigationService NavigationService => _shellNavigationService;

    private readonly INavigationService _shellNavigationService;

    public bool _onNavigatedTo = false;

    public ShellViewModel(INavigationService shellNavigationService)
    {
        _shellNavigationService = shellNavigationService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is string pageKey)
        {
            _shellNavigationService.NavigateTo(pageKey);
            _onNavigatedTo = true;
        }
        else _shellNavigationService.NavigateTo("HomePage");
    }
}
