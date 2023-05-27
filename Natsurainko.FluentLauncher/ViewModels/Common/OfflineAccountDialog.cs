using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components.Mvvm;
using System;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

public partial class OfflineAccountDialog : DialogViewModel
{
    public Action<IAccount> SetAccountAction { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string name;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    private string uuid;

    protected override bool EnableConfirmButton()
    {
        if (string.IsNullOrEmpty(Name))
            return false;

        if (!string.IsNullOrEmpty(Uuid))
            return Guid.TryParse(Uuid, out var _);

        return true;
    }

    protected override void OnConfirm(ContentDialog dialog)
    {
        var authenticator = string.IsNullOrEmpty(Uuid)
            ? new OfflineAuthenticator(Name)
            : new OfflineAuthenticator(Name, Guid.Parse(Uuid));

        SetAccountAction(authenticator.Authenticate());

        base.OnConfirm(dialog);
    }
}
