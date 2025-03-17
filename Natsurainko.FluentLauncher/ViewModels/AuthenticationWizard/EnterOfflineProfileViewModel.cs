using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Authentication;
using System;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class EnterOfflineProfileViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authenticationService;

    public EnterOfflineProfileViewModel()
    {
        XamlPageType = typeof(EnterOfflineProfilePage);

        _authenticationService = App.GetService<AuthenticationService>();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial string Name { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial string Uuid { get; set; }

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

    public override WizardViewModelBase GetNextViewModel()
        => new ConfirmProfileViewModel(_ => Task.FromResult<Account[]>([_authenticationService.LoginOffline(Name, Uuid)]));
}
