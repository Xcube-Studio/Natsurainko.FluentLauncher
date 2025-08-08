using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class CreateLaunchScriptDialog : ContentDialog
{
    CreateLaunchScriptDialogViewModel VM => (CreateLaunchScriptDialogViewModel)DataContext;

    public CreateLaunchScriptDialog()
    {
        InitializeComponent();
    }

    private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        => VM.SelectFilePath();
}
