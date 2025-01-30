using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ConfirmProfilePage : Page
{
    ConfirmProfileViewModel VM => (ConfirmProfileViewModel)DataContext;

    public ConfirmProfilePage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ItemsView.SelectionChanged += ItemsView_SelectionChanged;
        ItemsView.Select(0);
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        VM.SelectedAccount = (Account)sender.SelectedItem!;
    }
}
