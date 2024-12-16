using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class DownloadResourceDialog : ContentDialog
{
    DownloadResourceDialogViewModel VM => (DownloadResourceDialogViewModel)DataContext;

    public DownloadResourceDialog()
    {
        XamlRoot = MainWindow.XamlRoot;

        InitializeComponent();
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var vm = this.DataContext as DownloadResourceDialogViewModel;
        vm!.SaveFile();
    }
}
