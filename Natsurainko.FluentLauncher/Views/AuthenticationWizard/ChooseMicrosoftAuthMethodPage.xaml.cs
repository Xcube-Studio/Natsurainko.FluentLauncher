using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

namespace Natsurainko.FluentLauncher.Views.AuthenticationWizard;

public sealed partial class ChooseMicrosoftAuthMethodPage : Page
{
    ChooseMicrosoftAuthMethodViewModel VM => (ChooseMicrosoftAuthMethodViewModel)DataContext;

    public ChooseMicrosoftAuthMethodPage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ItemsView.Select(VM.MicrosoftAuthMethods.IndexOf(VM.SelectedMicrosoftAuthMethod));
        ItemsView.SelectionChanged += ItemsView_SelectionChanged;
    }

    private void ItemsView_SelectionChanged(ItemsView sender, ItemsViewSelectionChangedEventArgs args)
    {
        VM.SelectedMicrosoftAuthMethod = (MicrosoftAuthMethod)sender.SelectedItem!;
    }

    #region Converter Methods

    internal static string GetMicrosoftAuthMethodTitle(MicrosoftAuthMethod authMethod)
    {
        return authMethod switch
        {
            MicrosoftAuthMethod.BuiltInBrowser => LocalizedStrings.Converters__MicrosoftAuthMethod_BuiltInBrowser_Title,
            _ => LocalizedStrings.Converters__MicrosoftAuthMethod_DeviceFlowCode_Title,
        };
    }

    internal static string GetMicrosoftAuthMethodDescription(MicrosoftAuthMethod authMethod)
    {
        return authMethod switch
        {
            MicrosoftAuthMethod.BuiltInBrowser => LocalizedStrings.Converters__MicrosoftAuthMethod_BuiltInBrowser_Description,
            _ => LocalizedStrings.Converters__MicrosoftAuthMethod_DeviceFlowCode_Description,
        };
    }

    internal static ControlTemplate GetMicrosoftAuthMethodIcon(MicrosoftAuthMethod authMethod)
    {
        return authMethod switch
        {
            MicrosoftAuthMethod.BuiltInBrowser => (App.Current.Resources["BuiltInBrowserIcon"] as ControlTemplate)!,
            _ => (App.Current.Resources["DeviceFlowCodeIcon"] as ControlTemplate)!,
        };
    }

    #endregion
}
