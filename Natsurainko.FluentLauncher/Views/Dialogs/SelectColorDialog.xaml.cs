using FluentLauncher.Infra.UI.Dialogs;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using Windows.UI;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class SelectColorDialog : ContentDialog, IDialogResultAware<Color>
{
    SelectColorDialogViewModel VM => (SelectColorDialogViewModel)DataContext;

    Color IDialogResultAware<Color>.Result => VM.Color;

    public SelectColorDialog()
    {
        InitializeComponent();
    }
}
