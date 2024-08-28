using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Instances;
using Nrk.FluentCore.Management;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class ConfigViewModel : ObservableObject, INavigationAware
{
    public bool inited = false;

    public ReadOnlyObservableCollection<Account> Accounts { get; private set; }

    public ObservableCollection<string> VmArguments { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    public GameConfig GameConfig { get; private set; }

    [ObservableProperty]
    private Account targetedAccount;

    public ConfigViewModel(AccountService accountService)
    {
        Accounts = accountService.Accounts;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        GameConfig = MinecraftInstance.GetConfig();
        VmArguments = new(GameConfig.VmParameters ?? []);

        LoadTargetedAccount();

        inited = true;
    }

    private void LoadTargetedAccount()
    {
        var requestAccount = GameConfig.Account;

        if (requestAccount == null) return;

        var matchAccount = Accounts.Where(account =>
        {
            if (!account.Type.Equals(requestAccount.Type)) return false;
            if (!account.Uuid.Equals(requestAccount.Uuid)) return false;
            if (!account.Name.Equals(requestAccount.Name)) return false;

            if (requestAccount is YggdrasilAccount yggdrasil)
            {
                if (!((YggdrasilAccount)account).YggdrasilServerUrl.Equals(yggdrasil.YggdrasilServerUrl))
                    return false;
            }

            return true;
        });

#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        if (matchAccount.Any())
            targetedAccount = matchAccount.First();
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
    }

    [RelayCommand]
    public async Task AddArgument()
    {
        await new AddVmArgumentDialog()
        {
            DataContext = new AddVmArgumentDialogViewModel(VmArguments.Add)
        }.ShowAsync();

        GameConfig.VmParameters = VmArguments.ToArray();
    }

    [RelayCommand]
    public void RemoveArgument(string arg)
    {
        VmArguments.Remove(arg);
        GameConfig.VmParameters = VmArguments.ToArray();
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (!inited) return;

        if (e.PropertyName == nameof(TargetedAccount))
            GameConfig.Account = TargetedAccount;
    }
}
