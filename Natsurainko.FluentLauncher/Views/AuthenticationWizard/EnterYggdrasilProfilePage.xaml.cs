using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using System;
using System.Web;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class EnterYggdrasilProfilePage : Page
{
    EnterYggdrasilProfileViewModel VM => (EnterYggdrasilProfileViewModel)DataContext;

    public EnterYggdrasilProfilePage()
    {
        InitializeComponent();
    }

    private void StackPanel_DragEnter(object _, Microsoft.UI.Xaml.DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void StackPanel_Drop(object _, Microsoft.UI.Xaml.DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;

        string text = string.Empty;
        string pattern = "authlib-injector:yggdrasil-server:";

        try { text = await e.DataView.GetTextAsync(); } catch { }

        if (text.StartsWith(pattern))
            VM.Url = HttpUtility.UrlDecode(text[pattern.Length..]);
    }
}
