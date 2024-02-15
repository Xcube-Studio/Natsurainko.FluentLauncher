using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels;

class ShellViewModel : INavigationAware
{
    public INavigationService NavigationService => _shellNavigationService;

    private readonly INavigationService _shellNavigationService;
    private readonly SettingsService _settings;

    public bool _onNavigatedTo = false;

    public ShellViewModel(INavigationService shellNavigationService, SettingsService settings)
    {
        _shellNavigationService = shellNavigationService;
        _settings = settings;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        if (parameter is string pageKey)
        {
            _shellNavigationService.NavigateTo(pageKey);
            _onNavigatedTo = true;
        }
        else
            _shellNavigationService.NavigateTo(_settings.UseNewHomePage ? "NewHomePage" : "HomePage");
    }
}
