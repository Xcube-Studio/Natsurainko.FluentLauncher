using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentLauncher.Infra.UI.Navigation;
using Natsurainko.FluentLauncher.Models.Launch;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Launch;
using Natsurainko.FluentLauncher.Utils.Extensions;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.Common;
using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.Management;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

#nullable disable
namespace Natsurainko.FluentLauncher.ViewModels.Cores.Manage;

internal partial class CoreSettingsViewModel : ObservableObject, INavigationAware
{
    private readonly GameService _gameService;
    private readonly AccountService _accountService;

    private GameInfo _gameInfo;
    private bool inited = false;

    public ReadOnlyObservableCollection<Account> Accounts { get; private set; }

    [ObservableProperty]
    private Account targetedAccount;

    public GameSpecialConfig GameSpecialConfig { get; set; }

    public ObservableCollection<string> VmArguments { get; set; }

    public CoreSettingsViewModel(GameService gameService, AccountService accountService)
    {
        _gameService = gameService;
        _accountService = accountService;
    }

    void INavigationAware.OnNavigatedTo(object? parameter)
    {
        if (parameter is not GameInfo _gameInfo)
            throw new ArgumentException("Invalid parameter type");
        
        GameSpecialConfig = _gameInfo.GetSpecialConfig();

        Accounts = _accountService.Accounts;
        LoadTargetedAccount();

        GameSpecialConfig.PropertyChanged += GameSpecialConfig_PropertyChanged;

        VmArguments = GameSpecialConfig.VmParameters != null ? new(GameSpecialConfig.VmParameters) : new();
        VmArguments.CollectionChanged += VmArguments_CollectionChanged;

        inited = true;
    }

    [RelayCommand]
    public async Task AddArgument()
    {
        await new AddVmArgumentDialog()
        {
            DataContext = new AddVmArgumentDialogViewModel(VmArguments.Add)
        }.ShowAsync();
    }

    [RelayCommand]
    public void RemoveArgument(string arg) => VmArguments.Remove(arg);

    [RelayCommand]
    public void UnloadEvent(object args)
    {
        inited = false;

        VmArguments.CollectionChanged -= VmArguments_CollectionChanged;
        GameSpecialConfig.PropertyChanged -= GameSpecialConfig_PropertyChanged;
    }

    private void LoadTargetedAccount()
    {
        if (GameSpecialConfig.Account != null)
        {
            var matchAccount = _accountService.Accounts.Where(account =>
            {
                if (!account.Type.Equals(GameSpecialConfig.Account.Type)) return false;
                if (!account.Uuid.Equals(GameSpecialConfig.Account.Uuid)) return false;
                if (!account.Name.Equals(GameSpecialConfig.Account.Name)) return false;

                if (GameSpecialConfig.Account is YggdrasilAccount yggdrasil)
                {
                    if (!((YggdrasilAccount)account).YggdrasilServerUrl.Equals(yggdrasil.YggdrasilServerUrl))
                        return false;
                }

                return true;
            });

#pragma warning disable MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
            if (matchAccount.Any()) targetedAccount = matchAccount.First();
#pragma warning restore MVVMTK0034 // Direct field reference to [ObservableProperty] backing field
        }
    }

    private void VmArguments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        GameSpecialConfig.VmParameters = VmArguments.ToArray();
    }

    private void GameSpecialConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NickName" && !string.IsNullOrEmpty(GameSpecialConfig.NickName))
        {
            if (_gameInfo.Equals(_gameService.ActiveGame))
                _gameService.ActiveGame.Name = GameSpecialConfig.NickName;

            _gameInfo.Name = GameSpecialConfig.NickName;
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!inited) return;

        if (e.PropertyName == nameof(TargetedAccount))
            GameSpecialConfig.Account = TargetedAccount with { AccessToken = string.Empty };
    }
}
