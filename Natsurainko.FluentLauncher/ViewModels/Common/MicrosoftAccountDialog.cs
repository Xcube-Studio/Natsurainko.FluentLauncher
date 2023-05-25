using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.Components;
using Natsurainko.FluentLauncher.Components.Mvvm;
using Natsurainko.FluentLauncher.Utils;
using System;
using System.ComponentModel;
using System.Web;

namespace Natsurainko.FluentLauncher.ViewModels.Common;

public partial class MicrosoftAccountDialog : DialogViewModel
{
    public Action<IAccount> SetAccountAction { get; set; }

    public ContentDialog ContentDialog { get; set; }

    [ObservableProperty]
    private Uri source = new("https://login.live.com/oauth20_authorize.srf?client_id=0844e754-1d2e-4861-8e2b-18059609badb&response_type=code&scope=XboxLive.signin%20offline_access&redirect_uri=https://login.live.com/oauth20_desktop.srf&prompt=login");

    [ObservableProperty]
    private Visibility webViewVisibility = Visibility.Visible;

    [ObservableProperty]
    private Visibility loadVisibility = Visibility.Collapsed;

    [ObservableProperty]
    private string description;

    [RelayCommand]
    public void ExecuteScript(object parameter) => App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
    {
        var e = parameter.As<WebView2, CoreWebView2NavigationCompletedEventArgs>();

        if (e.args.IsSuccess)
            await e.sender.ExecuteScriptAsync(
            "document.querySelector('body').style.overflow='scroll';" +
            "var style=document.createElement('style');" +
            "style.type='text/css';" +
            "style.innerHTML='::-webkit-scrollbar{display:none}';" +
            "document.getElementsByTagName('body')[0].appendChild(style)");
    });

    [RelayCommand]
    public void CancelClick() => OnCancel();

    private bool onloading = false;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Source) && !onloading)
        {
            if (Source.ToString().Contains("error=access_denied"))
                OnCancel();
            else if (Source.ToString().Contains("error="))
                OnError(HttpUtility.UrlDecode(Source.Query.Split("&error_description=")[1]));
            else if (Source.ToString().Contains("code="))
                OnCode(Source.Query.Replace("?code=", string.Empty).Split('&')[0]);
        }
    }

    private void OnCancel()
    {
        ContentDialog.Hide();
        MessageService.Show("Cancelled Add Microsoft Account");
    }

    private void OnError(string error)
    {
        ContentDialog.Hide();
        MessageService.ShowError("Failed to Add Microsoft Account", error);
    }

    private async void OnCode(string code)
    {
        onloading = true;

        WebViewVisibility = Visibility.Collapsed;
        LoadVisibility = Visibility.Visible;

        try
        {
            var authenticator = new MicrosoftAuthenticator(code, "0844e754-1d2e-4861-8e2b-18059609badb", "https://login.live.com/oauth20_desktop.srf");
            authenticator.ProgressChanged += (_, e) => ContentDialog.DispatcherQueue.TryEnqueue(() => Description = e.Item2);

            SetAccountAction(await authenticator.AuthenticateAsync());
        }
        catch (Exception ex)
        {
            MessageService.ShowException(ex, "Failed to Add Microsoft Account");
        }

        ContentDialog.Hide();
    }
}
