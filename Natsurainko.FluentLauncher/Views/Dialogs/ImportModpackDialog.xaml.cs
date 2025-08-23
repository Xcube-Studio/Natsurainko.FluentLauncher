using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Dialogs;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Views.Dialogs;

public sealed partial class ImportModpackDialog : ContentDialog
{
    ImportModpackDialogViewModel VM => (ImportModpackDialogViewModel)DataContext;

    public ImportModpackDialog()
    {
        InitializeComponent();
    }

    private void Button_DragEnter(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
    }

    private async void Button_Drop(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            foreach (var item in await e.DataView.GetStorageItemsAsync())
            {
                VM.TryParseModpack(item.Path);
                if (VM.ModpackParsed) break;
            }
        }
    }
}
