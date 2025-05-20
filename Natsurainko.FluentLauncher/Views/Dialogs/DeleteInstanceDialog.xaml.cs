using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.Settings;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class DeleteInstanceDialog : ContentDialog
{
    DeleteInstanceDialogViewModel VM => (DeleteInstanceDialogViewModel)DataContext;

    public DeleteInstanceDialog(SettingsService settingsService)
    {
        InitializeComponent();
    }
}
