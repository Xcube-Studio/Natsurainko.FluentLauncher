using Natsurainko.FluentCore.Class.Model.Auth;
using Natsurainko.FluentLauncher.Class.AppData;
using Natsurainko.FluentLauncher.Class.Component;
using Natsurainko.FluentLauncher.Class.ViewData;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace Natsurainko.FluentLauncher.ViewModel.Pages.Settings;

public class SettingAccountPageVM : ViewModelBase<Page>
{
    public SettingAccountPageVM(Page control) : base(control)
    {
        Accounts = ConfigurationManager.AppSettings.Accounts.CreateCollectionViewData<Account, AccountViewData>();
        CurrentAccount = ConfigurationManager.AppSettings.CurrentAccount?.CreateViewData<Account, AccountViewData>();

        EnableDemoUser = ConfigurationManager.AppSettings.EnableDemoUser ?? (bool)AppSettings.Default.EnableDemoUser;
    }

    [Reactive]
    public ObservableCollection<AccountViewData> Accounts { get; set; }

    [Reactive]
    public AccountViewData CurrentAccount { get; set; }

    [Reactive]
    public bool EnableDemoUser { get; set; }

    public override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        DispatcherHelper.RunAsync(() =>
        {
            ConfigurationManager.AppSettings.Accounts = Accounts.Select(x => x.Data).ToList();
            ConfigurationManager.AppSettings.CurrentAccount = CurrentAccount?.Data;
            ConfigurationManager.AppSettings.EnableDemoUser = EnableDemoUser;

            ConfigurationManager.Configuration.Save();
        });
    }
}
