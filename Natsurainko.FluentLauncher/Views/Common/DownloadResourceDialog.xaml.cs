using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class DownloadResourceDialog : ContentDialog
{
    public DownloadResourceDialog()
    {
       InitializeComponent();
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var vm = this.DataContext as DownloadResourceDialogViewModel;
        vm!.SaveFile();
    }
}
