using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components.Mvvm;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.Dialogs;

public partial class OfflineAccountDialog : DialogViewModel
{
    public Action<IAccount> SetAccountAction { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string name;

    protected override bool EnableConfirmButton()
        => !string.IsNullOrEmpty(Name);

    protected override void OnConfirm(ContentDialog dialog)
    {
        var authenticator = new OfflineAuthenticator(Name);
        SetAccountAction(authenticator.Authenticate());

        base.OnConfirm(dialog);
    }
}
