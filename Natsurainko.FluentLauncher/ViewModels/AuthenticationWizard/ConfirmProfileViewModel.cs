using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.WindowsAppSDK.Runtime.Packages;
using Natsurainko.FluentCore.Interface;
using Natsurainko.FluentCore.Model.Auth;
using Natsurainko.FluentCore.Module.Authenticator;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Natsurainko.FluentLauncher.Views.AuthenticationWizard;
using Natsurainko.Toolkits.Values;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;
internal partial class ConfirmProfileViewModel : WizardViewModelBase
{
    public override bool CanPrevious => Loading == Visibility.Collapsed;

    public override bool CanNext => Loading == Visibility.Collapsed && SelectedAccount != null;

    private readonly Func<IEnumerable<IAccount>> _authenticateAction;

    public ObservableCollection<IAccount> Accounts { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private Visibility loading = Visibility.Collapsed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanNext))]
    private IAccount selectedAccount;

    public ConfirmProfileViewModel(Func<IEnumerable<IAccount>> authenticateAction)
    {
        XamlPageType = typeof(ConfirmProfilePage);
        _authenticateAction = authenticateAction;

        Accounts = new();

        Task.Run(() =>
        {
            App.MainWindow.DispatcherQueue.TryEnqueue(() => Loading = Visibility.Visible);
            var accountsList = new List<IAccount>(_authenticateAction());
            App.MainWindow.DispatcherQueue.TryEnqueue(() => 
            {
                Loading = Visibility.Collapsed;
                accountsList.ForEach(account => Accounts.Add(account));
            });
        }).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {

            }
        });
    }
}
