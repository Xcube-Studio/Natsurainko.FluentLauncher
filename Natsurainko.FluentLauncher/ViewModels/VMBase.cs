using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.WinUI.Mvvm;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Messaging;

namespace Natsurainko.FluentLauncher.ViewModels;

internal partial class PageVM<TPage> : ObservableRecipient, IViewAssociated<TPage>
    where TPage : Page
{
    public TPage View { get; set; } = null!;

    public DispatcherQueue Dispatcher { get; set; } = null!;

    //public INavigationService Navigation { get; } = navigationService;

    public virtual void OnLoaded() 
    {
        IsActive = true;
    }

    public virtual void OnUnloaded() 
    {
        IsActive = false;
    }

    protected void GlobalNavigate(string pageKey, object? parameter = null)
        => this.Messenger.Send(new GlobalNavigationMessage(pageKey, parameter));
}

internal partial class SettingsPageVM<TPage> : PageVM<TPage>
    where TPage : Page
{
    ~SettingsPageVM()
    {
        if (this is ISettingsViewModel settingsViewModel)
            settingsViewModel.RemoveSettingsChagnedHandlers();
    }
}

internal partial class DialogVM<TDialog> : ObservableRecipient, IViewAssociated<TDialog>
    where TDialog : ContentDialog
{
    public TDialog View { get; set; } = null!;

    public DispatcherQueue Dispatcher { get; set; } = null!;

    public virtual void OnLoaded() { }

    public virtual void OnUnloaded() { }
}