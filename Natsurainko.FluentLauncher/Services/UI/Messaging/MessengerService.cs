using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.Settings;
using Microsoft.UI.Xaml;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

/// <summary>
/// A singleton service for managing events subscription of other components and global messaging
/// </summary>
class MessengerService(AccountService accountService, SettingsService settingsService)
{
    static WeakReferenceMessenger Messenger => WeakReferenceMessenger.Default;

    public void SubscribeEvents()
    {
        accountService.ActiveAccountChanged += AccountService_ActiveAccountChanged;

        settingsService.ActiveMinecraftFolderChanged += SettingsService_SettingsStringValueChanged;
        settingsService.ActiveJavaChanged += SettingsService_SettingsStringValueChanged;

        settingsService.DisplayThemeChanged += SettingsService_DisplayThemeChanged;
    }

    private void AccountService_ActiveAccountChanged(object? sender, Nrk.FluentCore.Authentication.Account? e)
    {
        if (e == null)
            return;

        App.DispatcherQueue.TryEnqueue(() => Messenger.Send(new ActiveAccountChangedMessage(e)));
    }

    private void SettingsService_SettingsStringValueChanged(SettingsContainer sender, SettingChangedEventArgs e)
        => Messenger.Send(new SettingsStringValueChangedMessage(e.NewValue?.ToString() ?? string.Empty, e.Path));

    private void SettingsService_DisplayThemeChanged(SettingsContainer sender, SettingChangedEventArgs e) 
        => Messenger.Send(new SettingsRequestThemeChangedMessage((ElementTheme)e.NewValue!));
}