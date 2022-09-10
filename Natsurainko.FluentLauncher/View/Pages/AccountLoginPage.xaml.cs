using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.View.Dialogs;
using Natsurainko.FluentLauncher.View.Pages.Settings;
using Natsurainko.FluentLauncher.ViewModel.Pages;
using Natsurainko.Toolkits.Text;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.View.Pages;

public sealed partial class AccountLoginPage : Page
{
    public AccountLoginPageVM ViewModel { get; private set; }

    public AuthenticatorTypeDialog AuthenticatorTypeDialog { get; private set; }

    public LoginDialog LoginDialog { get; private set; }

    public AccountLoginPage()
    {
        this.InitializeComponent();

        ViewModel = ViewModelBuilder.Build<AccountLoginPageVM, Page>(this);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        AuthenticatorTypeDialog = new AuthenticatorTypeDialog();
        await AuthenticatorTypeDialog.ShowAsync();

        if (AuthenticatorTypeDialog.Canceled)
        {
            MainContainer.ContentFrame.Navigate(typeof(SettingsPage), typeof(SettingAccountPage));
            return;
        }

        if (AuthenticatorTypeDialog.ViewModel.CurrentAuthenticatorType == 1)
        {
            WebView2.NavigationCompleted += WebView2_NavigationCompleted;
            WebView2.NavigationStarting += WebView2_NavigationStarting;
            WebView2.Source = new Uri("https://login.live.com/oauth20_authorize.srf?client_id=0844e754-1d2e-4861-8e2b-18059609badb&response_type=code&redirect_uri=http://localhost:5001/fluentlauncher/auth-response&scope=XboxLive.signin%20offline_access&prompt=select_account");
            WebView2.Visibility = ViewModel.WebBrowserLoading = Visibility.Visible;
        }
        else
        {
            LoginDialog = new LoginDialog((int)AuthenticatorTypeDialog.ViewModel.CurrentAuthenticatorType, null);
            await LoginDialog.ShowAsync();
            MainContainer.ContentFrame.Navigate(typeof(SettingsPage), typeof(SettingAccountPage));
        }
    }

    private void WebView2_NavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        if (args.Uri.StartsWith("http://localhost:5001/fluentlauncher"))
        {
            var uri = new Uri(args.Uri);

            string code = uri.GetParameterUrl("code");
            string error = uri.GetParameterUrl("error");
            string error_description = uri.GetParameterUrl("error_description", true);

            WebView2.Visibility = Visibility.Collapsed;
            WebView2.NavigationStarting -= WebView2_NavigationStarting;

            if (error == null && !string.IsNullOrEmpty(code))
            {
                LoginDialog = new LoginDialog((int)AuthenticatorTypeDialog.ViewModel.CurrentAuthenticatorType, code);
                _ = LoginDialog.ShowAsync();
            }
            else
            {
                MainContainer.ShowInfoBarAsync($"登录账户失败：{error}", error_description, severity: Muxc.InfoBarSeverity.Error);
            }
        }
    }

    private void WebView2_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        ViewModel.WebBrowserLoading = Visibility.Collapsed;
        WebView2.NavigationCompleted -= WebView2_NavigationCompleted;
    }
}
