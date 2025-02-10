using CommunityToolkit.Mvvm.Messaging.Messages;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.ViewModels.Common;
using Nrk.FluentCore.Authentication;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

class TrackLaunchTaskChangedMessage : ValueChangedMessage<LaunchTaskViewModel?>
{
    public TrackLaunchTaskChangedMessage(LaunchTaskViewModel? value) : base(value) { }
}

class ActiveAccountChangedMessage : ValueChangedMessage<Account>
{
    public ActiveAccountChangedMessage(Account value) : base(value) { }
}

class SettingsStringValueChangedMessage : ValueChangedMessage<string>
{
    public string PropertyName { get; set; }

    public SettingsStringValueChangedMessage(string value, string propertyName) : base(value)
    {
        PropertyName = propertyName;
    }
}

class AccountSkinCacheUpdatedMessage : ValueChangedMessage<Account>
{
    public AccountSkinCacheUpdatedMessage(Account value) : base(value) { }
}

class InstanceLoaderSelectedMessage : ValueChangedMessage<List<InstanceLoaderItem>>
{
    public InstanceLoaderSelectedMessage(List<InstanceLoaderItem> list) : base(list) { }
}

class InstanceLoaderQueryMessage { }

class GlobalNavigationMessage : ValueChangedMessage<string>
{
    public GlobalNavigationMessage(string pageKey) : base(pageKey) { }
}