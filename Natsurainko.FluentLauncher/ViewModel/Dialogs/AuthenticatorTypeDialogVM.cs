using Natsurainko.FluentLauncher.Class.Component;
using ReactiveUI.Fody.Helpers;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Dialogs;

public class AuthenticatorTypeDialogVM : ViewModelBase<ContentDialog>
{
    public AuthenticatorTypeDialogVM(ContentDialog control) : base(control)
    {
        AuthenticatorTypes = ConfigurationManager.AppSettings.CurrentLanguage.GetString("ALP_CB_IS").Split(':');
    }

    [Reactive]
    public string[] AuthenticatorTypes { get; set; }

    [Reactive]
    public int? CurrentAuthenticatorType { get; set; } = 0;

    [Reactive]
    public bool ConfirmButtonEnable { get; set; } = true;

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentAuthenticatorType))
            ConfirmButtonEnable = CurrentAuthenticatorType != null;
    }
}
