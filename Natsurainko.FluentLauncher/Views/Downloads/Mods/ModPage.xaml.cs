using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.Downloads.Mods;

namespace Natsurainko.FluentLauncher.Views.Downloads.Mods;

public sealed partial class ModPage : Page, IBreadcrumbBarAware
{
    string IBreadcrumbBarAware.Route => "Mod";

    ModViewModel VM => (ModViewModel)DataContext;

    public MarkdownConfig MarkdownConfig { get; set; } = new MarkdownConfig();

    public ModPage()
    {
        this.InitializeComponent();
    }

    private void FilesItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args) => VM.SelectedFile = sender.SelectedItem;

    private void FilesItemsView_Unloaded(object sender, RoutedEventArgs e) => VM.SelectedFile = null;
}
