using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Instances;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Instances;

internal partial class ConfigViewModel(AccountService accountService, IDialogActivationService<ContentDialogResult> dialogs) 
    : PageVM, INavigationAware
{

    public ReadOnlyObservableCollection<Account> Accounts { get; private set; } = accountService.Accounts;

    public ObservableCollection<string> VmArguments { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    [ObservableProperty]
    public partial InstanceConfig InstanceConfig { get; set; }

    [ObservableProperty]
    public partial Account TargetedAccount { get; set; }

    private readonly IDialogActivationService<ContentDialogResult> _dialogs = dialogs;

    public bool inited = false;

    partial void OnTargetedAccountChanged(Account value)
    {
        if (inited)
            InstanceConfig.Account = TargetedAccount;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        InstanceConfig = MinecraftInstance.GetConfig();
        VmArguments = [.. InstanceConfig.VmParameters ?? []];

        LoadTargetedAccount();

        inited = true;
    }

    private void LoadTargetedAccount()
    {
        var requestAccount = InstanceConfig.Account;

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

        if (matchAccount.Any())
            TargetedAccount = matchAccount.First();
    }

    [RelayCommand]
    async Task AddArgument()
    {
        await _dialogs.ShowAsync("AddVmArgumentDialog", (object)VmArguments.Add);
        InstanceConfig.VmParameters = [.. VmArguments];
    }

    [RelayCommand]
    void RemoveArgument(string arg)
    {
        VmArguments.Remove(arg);
        InstanceConfig.VmParameters = [.. VmArguments];
    }
}
