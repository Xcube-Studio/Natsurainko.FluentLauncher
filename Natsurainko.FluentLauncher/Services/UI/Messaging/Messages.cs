using CommunityToolkit.Mvvm.Messaging.Messages;
using Natsurainko.FluentCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

class ActiveAccountChangedMessage : ValueChangedMessage<IAccount>
{
    public ActiveAccountChangedMessage(IAccount value) : base(value) { }
}
