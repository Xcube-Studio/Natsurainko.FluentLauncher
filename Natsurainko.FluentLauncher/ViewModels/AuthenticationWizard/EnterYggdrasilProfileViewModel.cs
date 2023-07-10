using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PInvoke.User32;
using System.Xml.Linq;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentCore.Interface;

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
        => new ConfirmProfileViewModel(() => _authenticationService.AuthenticateYggdrasil(Url, Email, Password).Select(x => (IAccount)x))
        {
            LoadingProgressText = "Authenticating with Yggdrasil Server"
        };
}
