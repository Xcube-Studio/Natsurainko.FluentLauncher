using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using System;
using System.Threading.Tasks;
using System.Web;
using Windows.ApplicationModel.DataTransfer;

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
    public partial string Url { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial string Email { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    public partial string Password { get; set; }

    public override WizardViewModelBase GetNextViewModel()
    {
        return new ConfirmProfileViewModel(async cancellationToken => await _authenticationService.LoginYggdrasilAsync(Url, Email, Password, cancellationToken)) 
            { LoadingProgressText = "Authenticating with Yggdrasil Server" };
    }

    [RelayCommand]
    void DragEnterEvent(object args)
    {
        var e = args.As<object, DragEventArgs>().args;
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    [RelayCommand]
    async Task DropEvent(object args)
    {
        var e = args.As<object, DragEventArgs>().args;
        e.AcceptedOperation = DataPackageOperation.Copy;

        string text = string.Empty;
        string pattern = "authlib-injector:yggdrasil-server:";

        try { text = await e.DataView.GetTextAsync(); } catch { }

        if (text.StartsWith(pattern))
            Url = HttpUtility.UrlDecode(text[pattern.Length..]);
    }
}
