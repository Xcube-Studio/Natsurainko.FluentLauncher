using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentLauncher.Class.Component;
using ReactiveUI.Fody.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace Natsurainko.FluentLauncher.Class.ViewData;

public class AccountViewData : ViewDataBase<Account>
{
    public AccountViewData(Account data) : base(data)
    {
        DispatcherHelper.RunAsync(() =>
        {
            AccountTypeName = ConfigurationManager.AppSettings.CurrentLanguage.GetString($"SAP_Converter_{data.Type}");

            YggdrasilServerUrl = data.Type switch
            {
                AccountType.Yggdrasil => (data as YggdrasilAccount).YggdrasilServerUrl,
                _ => string.Empty
            };
        });
    }

    [Reactive]
    public BitmapImage Icon { get; private set; }

    [Reactive]
    public string AccountTypeName { get; private set; }

    [Reactive]
    public string YggdrasilServerUrl { get; private set; }

    public Visibility UrlVisibility => this.Data.Type == AccountType.Yggdrasil
        ? Visibility.Visible : Visibility.Collapsed;

    public override int GetHashCode()
        => this.Data.Name.GetHashCode() ^ this.Data.Uuid.GetHashCode()
        ^ this.Data.AccessToken.GetHashCode() ^ this.Data.ClientToken.GetHashCode()
        ^ this.Data.Type.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        var item = (AccountViewData)obj;

        return this.Data.Name == item.Data.Name
            && this.Data.Uuid == item.Data.Uuid
            && this.Data.AccessToken == item.Data.AccessToken
            && this.Data.ClientToken == item.Data.ClientToken
            && this.Data.Type == item.Data.Type;
    }
}
