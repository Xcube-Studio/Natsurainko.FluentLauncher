using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class DownloadResourceDialog : ContentDialog
{
    DownloadResourceDialogViewModel VM => (DownloadResourceDialogViewModel)DataContext;

    public DownloadResourceDialog(SettingsService settingsService)
    {
        InitializeComponent();
        RequestedTheme = (ElementTheme)settingsService.DisplayTheme;
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var vm = this.DataContext as DownloadResourceDialogViewModel;
        vm!.SaveFile();
    }
}
