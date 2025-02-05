﻿using CommunityToolkit.Mvvm.Messaging.Messages;
using Nrk.FluentCore.Authentication;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

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

class GlobalNavigationMessage : ValueChangedMessage<string>
{
    public GlobalNavigationMessage(string pageKey) : base(pageKey) { }
}