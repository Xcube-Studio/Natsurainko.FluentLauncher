using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class EnterOfflineProfileViewModel : WizardViewModelBase
{
    public override bool CanNext
    {
        get
        {
            if (string.IsNullOrEmpty(Name))
                return false;

            if (!string.IsNullOrEmpty(Uuid))
                return Guid.TryParse(Uuid, out var _);

            return true;
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string name;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string uuid;

    private AuthenticationService _authenticationService;

    public EnterOfflineProfileViewModel()
    {
        XamlPageType = typeof(EnterOfflineProfilePage);

        _authenticationService = App.GetService<AuthenticationService>();
    }

    public override WizardViewModelBase GetNextViewModel()
        => new ConfirmProfileViewModel(() => new Account[] { _authenticationService.AuthenticateOffline(Name, Uuid) });
}
