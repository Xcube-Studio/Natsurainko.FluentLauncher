using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Instances;

namespace Natsurainko.FluentLauncher.Views.Instances;

public sealed partial class SavePage : Page, IBreadcrumbBarAware
{
    public string Route => "Save";

    internal SaveViewModel VM => (SaveViewModel)DataContext;

    public SavePage()
    {
        InitializeComponent();
    }

    private void MenuFlyout_Opened(object sender, object e)
    {
        if (sender is not MenuFlyout menuFlyout) return;

        foreach (var item in menuFlyout.Items)
        {
            if (item is MenuFlyoutItem menuItem)
            {
                menuItem.Command = (menuItem.Tag as string) switch
                {
                    "Open" => VM.OpenSaveFolderCommand,
                    "Backup" => VM.BackupSaveCommand,
                    "Delete" => VM.ConfirmDeleteCommand,
                    _ => menuItem.Command
                };
            }
        }
    }
}
