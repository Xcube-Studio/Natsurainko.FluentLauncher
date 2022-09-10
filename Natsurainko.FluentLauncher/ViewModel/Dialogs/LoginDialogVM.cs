using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentCore.Class.Model.Auth.Yggdrasil;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.View.Dialogs;
using Natsurainko.FluentLauncher.View.Pages.Settings;
using ReactiveUI.Fody.Helpers;
using System;
using Windows.UI.Xaml;

namespace Natsurainko.FluentLauncher.ViewModel.Dialogs;

public class LoginDialogVM : ViewModelBase<LoginDialog>
{
    public LoginDialogVM(LoginDialog control) : base(control)
    {

    }

    public string AccessToken { get; set; }

    [Reactive]
    public int? CurrentAuthenticatorType { get; set; }

    [Reactive]
    public string YggdrasilServerUrl { get; set; }

    [Reactive]
    public string Email { get; set; }

    [Reactive]
    public string Password { get; set; }

    [Reactive]
    public string Title { get; set; }

    [Reactive]
    public float Progress { get; set; }

    [Reactive]
    public string Message { get; set; }

    [Reactive]
    public Visibility UrlBoxVisibility { get; set; }

    [Reactive]
    public Visibility EmailBoxVisibility { get; set; }

    [Reactive]
    public Visibility PasswordBoxVisibility { get; set; }

    [Reactive]
    public Visibility PlaceholderVisibity { get; set; }

    [Reactive]
    public bool ControlEnable { get; set; } = true;

    public void SetType(int currentAuthenticatorType)
    {
        CurrentAuthenticatorType = currentAuthenticatorType;
        Title = ConfigurationManager.AppSettings.CurrentLanguage.GetString("ALP_CB_IS").Split(':')[(int)CurrentAuthenticatorType];

        switch ((int)currentAuthenticatorType)
        {
            case 0:
                UrlBoxVisibility = PlaceholderVisibity = PasswordBoxVisibility = Visibility.Collapsed;
                EmailBoxVisibility = Visibility.Visible;
                break;
            case 1:
                UrlBoxVisibility = EmailBoxVisibility = PasswordBoxVisibility = Visibility.Collapsed;
                PlaceholderVisibity = Visibility.Visible;

                this.Control.ConfirmButton_Click(null, null);
                break;
            case 2:
                UrlBoxVisibility = EmailBoxVisibility = PasswordBoxVisibility = Visibility.Visible;
                PlaceholderVisibity = Visibility.Collapsed;
                break;
        }
    }

    public async void MicrosoftAuthenticate()
    {
        ControlEnable = false;

        void Authenticator_ProgressChanged(object sender, (float, string) e) => DispatcherHelper.RunAsync(() =>
        {
            Progress = e.Item1;
            Message = e.Item2;
        });

        try
        {
            var authenticator = new MicrosoftAuthenticator(this.Control.AccessCode, GlobalResources.ClientId, GlobalResources.RedirectUri);
            authenticator.ProgressChanged += Authenticator_ProgressChanged;
            var account = await authenticator.AuthenticateAsync();
            authenticator.ProgressChanged -= Authenticator_ProgressChanged;

            ConfigurationManager.AppSettings.Accounts.Add(account);
            ConfigurationManager.AppSettings.CurrentAccount = account;

            ConfigurationManager.Configuration.Save();

            MainContainer.ShowInfoBarAsync($"添加账户成功：微软账户 {account.Name}", $"欢迎回来, {account.Name}", severity: InfoBarSeverity.Success);
            MainContainer.ContentFrame.Navigate(typeof(SettingsPage), typeof(SettingAccountPage));
        }
        catch (Exception ex)
        {
            MainContainer.ShowInfoBarAsync($"验证账户失败：{ex.Source}", $"{ex.Message}\r\n{ex.StackTrace}", severity: InfoBarSeverity.Error);
        }

        ControlEnable = true;

        this.Control.Hide();
    }

    public async void YggdrasilAuthenticate()
    {
        ControlEnable = false;

        try
        {
            var authenticator = new YggdrasilAuthenticator(YggdrasilAuthenticatorMethod.Login, yggdrasilServerUrl: $"{YggdrasilServerUrl}/authserver", email: Email, password: Password);
            var account = await authenticator.AuthenticateAsync();

            ConfigurationManager.AppSettings.Accounts.Add(account);
            ConfigurationManager.AppSettings.CurrentAccount = account;

            ConfigurationManager.Configuration.Save();

            MainContainer.ShowInfoBarAsync($"添加账户成功：外置账户 {account.Name}", $"欢迎回来, {account.Name}", severity: InfoBarSeverity.Success);
        }
        catch (Exception ex)
        {
            MainContainer.ShowInfoBarAsync($"验证账户失败：{ex.Source}", $"{ex.Message}\r\n{ex.StackTrace}", severity: InfoBarSeverity.Error);
        }

        ControlEnable = true;

        this.Control.Hide();
    }

    public void OfflineAuthenticate()
    {
        var account = new OfflineAuthenticator(this.Email).Authenticate();

        ConfigurationManager.AppSettings.Accounts.Add(account);
        ConfigurationManager.AppSettings.CurrentAccount = account;

        ConfigurationManager.Configuration.Save();

        MainContainer.ShowInfoBarAsync($"添加账户成功：离线账户 {account.Name}", $"欢迎回来, {account.Name}", severity: InfoBarSeverity.Success);

        this.Control.Hide();
    }
}
