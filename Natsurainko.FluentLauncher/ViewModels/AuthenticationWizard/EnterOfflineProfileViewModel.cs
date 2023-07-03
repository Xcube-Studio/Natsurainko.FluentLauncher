using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public EnterOfflineProfileViewModel()
    {
        XamlPageType = typeof(EnterOfflineProfilePage);
    }

    public override WizardViewModelBase GetNextViewModel() 
    {
        var authenticator = string.IsNullOrEmpty(Uuid)
            ? new OfflineAuthenticator(Name)
            : new OfflineAuthenticator(Name, Guid.Parse(Uuid));

        return new ConfirmProfileViewModel(authenticator);
    }
}
