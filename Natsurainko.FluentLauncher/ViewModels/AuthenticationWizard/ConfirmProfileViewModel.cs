using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.ViewModels.AuthenticationWizard;

internal partial class ConfirmProfileViewModel : WizardViewModelBase
{
    private readonly IAuthenticator _authenticator;

    public ObservableCollection<IAccount> Accounts { get; init; }

    [ObservableProperty]
    private string title;  

    public ConfirmProfileViewModel(IAuthenticator authenticator)
    {
        XamlPageType = typeof(ConfirmProfilePage);
        _authenticator = authenticator;
        Accounts = new();

        Title = authenticator is MicrosoftAuthenticator ? "Confirm Minecraft Profile (4/4)" : "Confirm Minecraft Profile (3/3)";

        Task.Run(async () =>
        {
            var accountsList = new List<IAccount>();

            if (authenticator is YggdrasilAuthenticator yggdrasilAuthenticator)
            {
                var account = await yggdrasilAuthenticator.AuthenticateAsync(profiles => Task.Run(() =>
                {
                    accountsList.AddRange(profiles.Select(x => (IAccount)new YggdrasilAccount
                    {
                        Name = x.Name,
                        Uuid = Guid.Parse(x.Id)
                    }));

                    return profiles.First();
                }));

                accountsList.ForEach(x =>
                {
                    x.AccessToken = account.AccessToken;
                    x.ClientToken = account.ClientToken;
                });
            }
            else
            {
                try
                {
                    accountsList.Add(await authenticator.AuthenticateAsync());
                }
                catch
                {
                    throw;
                }
            }

            App.MainWindow.DispatcherQueue.TryEnqueue(() => accountsList.ForEach(x => Accounts.Add(x)));
        });
    }
}
