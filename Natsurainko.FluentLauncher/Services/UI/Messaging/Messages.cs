using CommunityToolkit.Mvvm.Messaging.Messages;
using Nrk.FluentCore.Classes.Datas.Authenticate;
using System;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

class ActiveAccountChangedMessage : ValueChangedMessage<Account>
{
    public ActiveAccountChangedMessage(Account value) : base(value) { }
}
