using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.Settings;
using Natsurainko.FluentLauncher.Services.Accounts;
using Natsurainko.FluentLauncher.Services.Settings;

namespace Natsurainko.FluentLauncher.Services.UI.Messaging;

/// <summary>
/// A singleton service for managing events subscription of other components and global messaging
/// </summary>
class MessengerService
{
    private readonly AccountService _accountService;
    private readonly SettingsService _settingsService;

    public MessengerService(AccountService accountService, SettingsService settingsService)
    {
        _accountService = accountService;
        _settingsService = settingsService;
    }

    public void SubscribeEvents()
    {
        _accountService.ActiveAccountChanged += AccountService_ActiveAccountChanged;

        _settingsService.ActiveMinecraftFolderChanged += SettingsService_SettingsStringValueChanged;
        _settingsService.ActiveJavaChanged += SettingsService_SettingsStringValueChanged;
    }

    private void AccountService_ActiveAccountChanged(object? sender, Nrk.FluentCore.Authentication.Account? e)
    {
        if (e == null)
            return;

        App.DispatcherQueue.TryEnqueue(() => WeakReferenceMessenger.Default.Send(new ActiveAccountChangedMessage(e)));
    }

    private void SettingsService_SettingsStringValueChanged(SettingsContainer sender, SettingChangedEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new SettingsStringValueChangedMessage(
            e.NewValue == null ? string.Empty : e.NewValue.ToString()!,
            e.Path));
    }
}
