using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores;

internal partial class ConfigViewModel : ObservableObject, INavigationAware
{
    public bool inited = false;

    public ReadOnlyObservableCollection<Account> Accounts { get; private set; }

    public ObservableCollection<string> VmArguments { get; private set; }

    public MinecraftInstance MinecraftInstance { get; private set; }

    [ObservableProperty]
    public partial InstanceConfig InstanceConfig { get; set; }

    [ObservableProperty]
    public partial Account TargetedAccount { get; set; }

    private readonly IDialogActivationService<ContentDialogResult> _dialogs;

    public ConfigViewModel(AccountService accountService, IDialogActivationService<ContentDialogResult> dialogs)
    {
        Accounts = accountService.Accounts;
        _dialogs = dialogs;
    }

    void INavigationAware.OnNavigatedTo(object parameter)
    {
        MinecraftInstance = parameter as MinecraftInstance;
        InstanceConfig = MinecraftInstance.GetConfig();
        VmArguments = new(InstanceConfig.VmParameters ?? []);

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

#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        if (matchAccount.Any())
            TargetedAccount = matchAccount.First();
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
    }

    [RelayCommand]
    public async Task AddArgument()
    {
        await _dialogs.ShowAsync("AddVmArgumentDialog", (object)VmArguments.Add);
        InstanceConfig.VmParameters = [.. VmArguments];
    }

    [RelayCommand]
    public void RemoveArgument(string arg)
    {
        VmArguments.Remove(arg);
        InstanceConfig.VmParameters = [.. VmArguments];
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (!inited) return;

        if (e.PropertyName == nameof(TargetedAccount))
            InstanceConfig.Account = TargetedAccount;
    }
}
