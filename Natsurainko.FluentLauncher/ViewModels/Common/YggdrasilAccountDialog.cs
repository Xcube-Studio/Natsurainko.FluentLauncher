using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

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

    [ObservableProperty]
    private bool selected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmRoleCommand))]
    private ProfileModel selectedProfile;

    [ObservableProperty]
    private IEnumerable<ProfileModel> profiles;

    [ObservableProperty]
    private Visibility selectListVisibility = Visibility.Collapsed;

    protected override bool EnableConfirmButton()
        => !(string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password));

    private bool EnableConfirmRole()
        => SelectedProfile != null;

    [RelayCommand(CanExecute = nameof(EnableConfirmRole))]
    public void ConfirmRole() => Selected = true;

    protected override void OnConfirm(ContentDialog dialog)
    {
        try
        {
            var authenticator = new YggdrasilAuthenticator(
                FluentCore.Model.Auth.AuthenticatorMethod.Login,
                email: Email,
                password: Password,
                yggdrasilServerUrl: Url);

            SetAccountAction(authenticator.Authenticate(async profiles =>
            {
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    SelectListVisibility = Visibility.Visible;
                    Profiles = profiles;
                });

                while (!Selected)
                    await Task.Delay(500);

                App.MainWindow.DispatcherQueue.TryEnqueue(() => SelectListVisibility = Visibility.Collapsed);

                return SelectedProfile;
            }));
        }
        catch (Exception ex)
        {
            MessageService.ShowException(ex, "Failed to Add Yggdrasil Account");
        }

        base.OnConfirm(dialog);
    }
}
