﻿using CommunityToolkit.Mvvm.ComponentModel;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class EnterYggdrasilProfileViewModel : WizardViewModelBase
{
    private readonly AuthenticationService _authenticationService;

    public override bool CanNext => !(string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password));

    public EnterYggdrasilProfileViewModel()
    {
        XamlPageType = typeof(EnterYggdrasilProfilePage);

        _authenticationService = App.GetService<AuthenticationService>();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string url;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string email;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string password;

    public override WizardViewModelBase GetNextViewModel()
    {
        return new ConfirmProfileViewModel(
            async cancellationToken => await _authenticationService.LoginYggdrasilAsync(Url, Email, Password)) 
            { LoadingProgressText = "Authenticating with Yggdrasil Server" };
    }
}
