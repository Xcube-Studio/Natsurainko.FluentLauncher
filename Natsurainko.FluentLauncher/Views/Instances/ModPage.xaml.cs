using CommunityToolkit.WinUI.Controls;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.UI.Notification;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Instances;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.DataTransfer;

namespace Natsurainko.FluentLauncher.Views.Instances;

public sealed partial class ModPage : Page, IBreadcrumbBarAware
{
    public string Route => "Mod";

    ModViewModel VM => (ModViewModel)DataContext;

    public ModPage()
    {
        InitializeComponent();
    }

    private void Page_DragEnter(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        App.GetService<INotificationService>().ModDrag();
    }

    private async void Page_Drop(object sender, DragEventArgs e)
    {
        int modCount = 0;
        e.AcceptedOperation = DataPackageOperation.Copy;

        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            foreach (var item in await e.DataView.GetStorageItemsAsync())
            {
                FileInfo fileInfo = new(item.Path);

                if (fileInfo.Extension != ".jar")
                    continue;

                fileInfo.CopyTo(Path.Combine(VM.ModsFolder, fileInfo.Name), true);
                modCount++;
            }
        }

        if (modCount > 0)
        {
            App.GetService<INotificationService>().ModAdded(modCount);
            Task.Run(VM.LoadModsAsync).Forget();
        }
    }

    private void SettingsCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is SettingsCard settingsCard)
            settingsCard.ContextFlyout.ShowAt(settingsCard);
    }

    private void MenuFlyout_Opened(object sender, object e)
    {
        MenuFlyout menuFlyout = (sender as MenuFlyout)!;
        ICommand?[] commands = [VM.OpenModCommand, VM.ConfirmDeleteCommand, VM.SearchMcModCommand];
        int index = 0;

        foreach (var item in menuFlyout.Items)
        {
            if (item is MenuFlyoutItem menuItem)
            {
                menuItem.Command = commands[index++];
            }
        }
    }

    internal static bool ShowMcModSearchOption() => ApplicationLanguages.PrimaryLanguageOverride == "zh-Hans" || ApplicationLanguages.PrimaryLanguageOverride == "zh-Hant";
}
