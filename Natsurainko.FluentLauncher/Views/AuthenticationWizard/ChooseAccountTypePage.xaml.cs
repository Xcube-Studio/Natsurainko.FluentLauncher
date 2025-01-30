using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ChooseAccountTypePage : Page
{
    // Note: The app will crash if two instances of this page are referencing the same VM after navigating back.
    // Changes on the page being displayed cause changes in the VM, which triggers an update on the page unloaded.
    // Radio buttons on the page unloaded are updated, but its DataContext is null, resulting in a crash.

    ChooseAccountTypeViewModel VM => (ChooseAccountTypeViewModel)DataContext;

    public ChooseAccountTypePage()
    {
        InitializeComponent();
    }
    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ItemsView.Select(VM.AccountTypes.IndexOf(VM.SelectedAccountType));
        ItemsView.SelectionChanged += ItemsView_SelectionChanged;
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        DataContext = new ChooseAccountTypeViewModel(null!);
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        VM.SelectedAccountType = sender.SelectedItem as AccountType?
            ?? AccountType.Microsoft;
    }

    #region Converter Methods

    internal static string GetAccountTypeTitle(AccountType accountType)
    {
        string account = LocalizedStrings.Converters__Account;

        if (!ApplicationLanguages.PrimaryLanguageOverride.StartsWith("zh-"))
            account = " " + account;

        return accountType switch
        {
            AccountType.Microsoft => LocalizedStrings.Converters__Microsoft + account,
            AccountType.Yggdrasil => LocalizedStrings.Converters__Yggdrasil + account,
            _ => LocalizedStrings.Converters__Offline + account,
        };
    }

    internal static string GetAccountTypeDescription(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Microsoft => LocalizedStrings.Converters__Microsoft_Description,
            AccountType.Yggdrasil => LocalizedStrings.Converters__Yggdrasil_Description,
            _ => LocalizedStrings.Converters__Offline_Description,
        };
    }

    internal static ControlTemplate GetAccountTypeIcon(AccountType accountType)
    {
        return accountType switch
        {
            AccountType.Microsoft => (App.Current.Resources["MicrosoftIcon"] as ControlTemplate)!,
            AccountType.Yggdrasil => (App.Current.Resources["YggdrasilIcon"] as ControlTemplate)!,
            _ => (App.Current.Resources["OfflineIcon"] as ControlTemplate)!,
        };
    }

    #endregion
}
