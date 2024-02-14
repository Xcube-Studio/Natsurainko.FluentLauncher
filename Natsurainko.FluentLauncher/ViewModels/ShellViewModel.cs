using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI.Navigation;

namespace Natsurainko.FluentLauncher.ViewModels;

class ShellViewModel : INavigationAware
{
    public INavigationService NavigationService => _shellNavigationService;

    private readonly INavigationService _shellNavigationService;
    private readonly SettingsService _settings;

    public ShellViewModel(INavigationService shellNavigationService, SettingsService settings)
    {
        _shellNavigationService = shellNavigationService;
        _settings = settings;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        _shellNavigationService.NavigateTo(_settings.UseNewHomePage ? "NewHomePage" : "HomePage");
    }

}
