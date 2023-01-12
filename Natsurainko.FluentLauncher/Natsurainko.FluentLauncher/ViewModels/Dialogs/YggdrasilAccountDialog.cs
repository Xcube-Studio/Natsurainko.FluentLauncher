using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Views.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

public partial class YggdrasilAccountDialog : DialogViewModel
{
    public Action<IAccount> SetAccountAction { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string url;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string email;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string password;

    protected override bool EnableConfirmButton()
        => !(string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password));

    protected override void OnConfirm(ContentDialog dialog)
    {
        try
        {
            var authenticator = new YggdrasilAuthenticator(
                FluentCore.Model.Auth.AuthenticatorMethod.Login,
                email: Email,
                password: Password,
                yggdrasilServerUrl: Url);

            SetAccountAction(authenticator.Authenticate());
        }
        catch (Exception ex)
        {
            MainContainer.ShowMessagesAsync(
                "Failed to Add Yggdrasil Account",
                ex.ToString(),
                InfoBarSeverity.Error);
        }

        base.OnConfirm(dialog);
    }
}
