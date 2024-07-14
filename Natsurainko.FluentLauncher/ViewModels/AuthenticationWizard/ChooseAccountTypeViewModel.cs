using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Authentication;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class ChooseAccountTypeViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authService;

    public override bool CanNext => SelectedAccountType != null;

    public override bool CanPrevious => false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private AccountType? selectedAccountType = AccountType.Microsoft;

    public ChooseAccountTypeViewModel(AuthenticationService authService)
    {
        XamlPageType = typeof(ChooseAccountTypePage);
        _authService = authService;
    }

    [RelayCommand]
    public void Checked(int index) => SelectedAccountType = (AccountType)index;

    public override WizardViewModelBase GetNextViewModel()
    {
        return SelectedAccountType switch
        {
            AccountType.Microsoft => new ChooseMicrosoftAuthMethodViewModel(_authService),
            AccountType.Offline => new EnterOfflineProfileViewModel(),
            AccountType.Yggdrasil => new EnterYggdrasilProfileViewModel()
        };
    }
}
