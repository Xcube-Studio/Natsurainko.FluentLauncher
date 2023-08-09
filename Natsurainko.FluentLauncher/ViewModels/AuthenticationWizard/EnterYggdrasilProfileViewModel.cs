using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class EnterYggdrasilProfileViewModel : WizardViewModelBase
{
    public override bool CanNext => !(string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password));

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string url;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string email;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string password;

    private AuthenticationService _authenticationService;

    public EnterYggdrasilProfileViewModel()
    {
        XamlPageType = typeof(EnterYggdrasilProfilePage);

        _authenticationService = App.GetService<AuthenticationService>();
    }

    public override WizardViewModelBase GetNextViewModel()
        => new ConfirmProfileViewModel(() => _authenticationService.AuthenticateYggdrasil(Url, Email, Password))
        {
            LoadingProgressText = "Authenticating with Yggdrasil Server"
        };
}
