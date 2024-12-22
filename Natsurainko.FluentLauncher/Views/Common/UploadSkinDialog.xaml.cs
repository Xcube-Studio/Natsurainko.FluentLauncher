using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class UploadSkinDialog : ContentDialog
{
    UploadSkinDialogViewModel VM => (UploadSkinDialogViewModel)DataContext;

    public UploadSkinDialog()
    {
        InitializeComponent();
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var vm = this.DataContext as UploadSkinDialogViewModel;
        vm!.BrowserFile();
    }
}
