using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class UploadSkinDialog : ContentDialog
{
    UploadSkinDialogViewModel VM => (UploadSkinDialogViewModel)DataContext;

    public UploadSkinDialog(SettingsService settingsService)
    {
        InitializeComponent();
    }
}
