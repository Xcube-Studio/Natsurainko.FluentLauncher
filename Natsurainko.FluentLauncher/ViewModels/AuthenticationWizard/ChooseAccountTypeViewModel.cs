using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Classes.Enums;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class ChooseAccountTypeViewModel : WizardViewModelBase
{
    public override bool CanNext => SelectedAccountType != null;

    public override bool CanPrevious => false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private AccountType? selectedAccountType;

    public ChooseAccountTypeViewModel()
    {
        XamlPageType = typeof(ChooseAccountTypePage);
    }

    [RelayCommand]
    public void Checked(string type)
    {
        SelectedAccountType = type switch
        {
            "Microsoft" => AccountType.Microsoft,
            "Yggdrasil" => AccountType.Yggdrasil,
            "Offline" => AccountType.Offline,
            _ => null
        };
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        return SelectedAccountType switch
        {
            AccountType.Microsoft => new ChooseMicrosoftAuthMethodViewModel(),
            AccountType.Offline => new EnterOfflineProfileViewModel(),
            AccountType.Yggdrasil => new EnterYggdrasilProfileViewModel(),
            _ => null
        };
    }
}
