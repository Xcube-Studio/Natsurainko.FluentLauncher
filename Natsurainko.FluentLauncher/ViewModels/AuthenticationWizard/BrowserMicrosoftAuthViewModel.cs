﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Nrk.FluentCore.Authentication;
using System;
using System.ComponentModel;
using System.Web;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class BrowserMicrosoftAuthViewModel : WizardViewModelBase
{
    public override bool CanNext => AccessCode != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private string accessCode;

    [ObservableProperty]
    private bool needRefresh;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private string icon;

    [ObservableProperty]
    private Uri source;

    private readonly string AuthUrl = "https://login.live.com/oauth20_authorize.srf" +
        $"?client_id={AuthenticationService.MicrosoftClientId}" +
        "&response_type=code" +
        "&scope=XboxLive.signin%20offline_access" +
        $"&redirect_uri={AuthenticationService.MicrosoftRedirectUrl}" +
        "&prompt=login";

    private readonly AuthenticationService _authenticationService;

    public BrowserMicrosoftAuthViewModel()
    {
        XamlPageType = typeof(BrowserMicrosoftAuthPage);

        _authenticationService = App.GetService<AuthenticationService>();

        Source = new(AuthUrl);
    }

    [RelayCommand]
    public void Refresh() => Source = new(AuthUrl);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Source))
            ParseUrl();
    }

    private void ParseUrl()
    {
        if (Source.AbsoluteUri == AuthUrl)
        {
            NeedRefresh = false;
            return;
        }

        if (!Source.AbsoluteUri.StartsWith("https://login.live.com/oauth20_desktop.srf?"))
            return;

        var nameValueCollection = HttpUtility.ParseQueryString(Source.Query);

        if (nameValueCollection.Get("error") is string error)
        {
            Icon = "\uE711";
            Description = $"{error}:{nameValueCollection.Get("error_description")}";
        }

        AccessCode = nameValueCollection.Get("code");
        if (AccessCode != null)
        {
            Icon = "\uE73E";
            Description = "Browser authentication succeeded";
        }

        NeedRefresh = true;
    }

    public override WizardViewModelBase GetNextViewModel()
    {
        ConfirmProfileViewModel confirmProfileViewModel = default;

        confirmProfileViewModel = new ConfirmProfileViewModel(() =>
        {
            var progress = new Progress<MicrosoftAuthenticationProgress>((p) =>
            {
                App.DispatcherQueue.TryEnqueue(() => confirmProfileViewModel.LoadingProgressText = p.ToString());
            });

            return [_authenticationService.LoginMicrosoftAsync(AccessCode, progress).GetAwaiter().GetResult()];
        });

        return confirmProfileViewModel;
    }
}
