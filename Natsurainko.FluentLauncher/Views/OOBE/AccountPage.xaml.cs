using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Globalization;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using Natsurainko.FluentLauncher.Utils;
using Natsurainko.FluentLauncher.ViewModels.OOBE;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Views.OOBE;

public sealed partial class AccountPage : Page, IRecipient<ActiveAccountChangedMessage>
{
    OOBEViewModel VM => (OOBEViewModel)DataContext;

    public AccountPage()
    {
        InitializeComponent();
    }

    private void Page_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.Register(this);

        this.Unloaded += Page_Unloaded;
    }

    private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);

        this.DataContext = null;
        ListView.ItemsSource = null;
    }

    async void IRecipient<ActiveAccountChangedMessage>.Receive(ActiveAccountChangedMessage message) => await DispatcherQueue.EnqueueAsync(() =>
    {
        VM.processingActiveAccountChangedMessage = true;
        VM.ActiveAccount = message.Value;
        VM.processingActiveAccountChangedMessage = false;
    });

    #region Converters Methods

    internal static string GetAccountTypeName(AccountType accountType)
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

    internal static string TryGetYggdrasilServerName(Account account)
    {
        if (account is YggdrasilAccount yggdrasilAccount)
        {
            if (yggdrasilAccount.MetaData.TryGetValue("server_name", out var serverName))
                return serverName;
        }

        return string.Empty;
    }

    #endregion
}
