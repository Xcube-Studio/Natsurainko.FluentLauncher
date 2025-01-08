using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Common;

namespace Natsurainko.FluentLauncher.Views.Common;

public sealed partial class UploadSkinDialog : ContentDialog
{
    public UploadSkinDialog(SettingsService _settingsService)
    {
        InitializeComponent();
        this.RequestedTheme = (ElementTheme)_settingsService.DisplayTheme;
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var vm = this.DataContext as UploadSkinDialogViewModel;
        vm!.BrowserFile();
    }
}
