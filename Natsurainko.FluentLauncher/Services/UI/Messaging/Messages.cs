using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Models.UI;
using Natsurainko.FluentLauncher.ViewModels;
using Nrk.FluentCore.Authentication;
using System;
using System.Collections.Generic;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

class TrackLaunchTaskChangedMessage(LaunchTaskViewModel? value) : ValueChangedMessage<LaunchTaskViewModel?>(value);

class ActiveAccountChangedMessage(Account value) : ValueChangedMessage<Account>(value);

class SettingsStringValueChangedMessage(string value, string propertyName) : ValueChangedMessage<string>(value)
{
    public string PropertyName { get; set; } = propertyName;
}

class LanguageChangedMessage(string value) : ValueChangedMessage<string>(value);

class SettingsRequestThemeChangedMessage(ElementTheme theme) : ValueChangedMessage<ElementTheme>(theme);

class InstanceLoaderSelectedMessage(List<InstanceLoaderItem> list) : ValueChangedMessage<List<InstanceLoaderItem>>(list);

class InstanceLoaderQueryMessage();

class SkinTextureUpdatedMessage(Account value) : ValueChangedMessage<Account>(value);

class CapeTextureUpdatedMessage(Account value) : ValueChangedMessage<Account>(value);

class BackgroundTaskCountChangedMessage();

class DownloadTaskCreatedMessage(int eventId) : ValueChangedMessage<int>(eventId);

class GlobalNavigationMessage(string pageKey, object? parameter = null) : ValueChangedMessage<string>(pageKey)
{
    public object? Parameter { get; init; } = parameter;
}

class ExceptionDialogRepeatedlyRequestMessage(Exception exception) : ValueChangedMessage<Exception>(exception);