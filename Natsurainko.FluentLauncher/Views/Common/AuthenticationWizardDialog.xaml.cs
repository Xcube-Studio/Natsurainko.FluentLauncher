using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.Services.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class AuthenticationWizardDialog : ContentDialog
{
    public AuthenticationWizardDialog(SettingsService _settingsService)
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }
}
