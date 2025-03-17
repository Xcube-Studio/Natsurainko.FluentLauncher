using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FluentLauncher.Infra.Settings.Mvvm;
using FluentLauncher.Infra.UI.Dialogs;
using FluentLauncher.Infra.UI.Navigation;
using FluentLauncher.Infra.WinUI.Mvvm;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Natsurainko.FluentLauncher.Services.UI.Messaging;
using System;
using System.Collections.ObjectModel;

namespace Natsurainko.FluentLauncher.ViewModels;

internal partial class PageVM : ObservableRecipient, IViewAssociated
{
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

internal partial class PageVM<TPage> : PageVM, IViewAssociated<TPage>
    where TPage : Page
{
    public TPage View { get; private set; } = null!;

    public void SetView(object view) => View = (TPage)view;
}

internal partial class SettingsPageVM : PageVM
{
    ~SettingsPageVM()
    {
        if (this is ISettingsViewModel settingsViewModel)
            settingsViewModel.RemoveSettingsChagnedHandlers();
    }
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

internal partial class NavigationPageVM(INavigationService navigationService) : PageVM, INavigationAware
{
    protected virtual string RootPageKey { get; set; } = null!;

    public INavigationService NavigationService { get; init; } = navigationService;

    public ObservableCollection<string> DisplayedPath { get; } = [];

    public virtual void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length == 1 && routes[0] == RootPageKey)
            NavigateTo($"{RootPageKey}/Default");
        else
            NavigateTo(string.Join('/', routes));
    }

    protected virtual void NavigateTo(string pageKey, object? parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter); // Default page

        if (pageKey == $"{RootPageKey}/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add(RootPageKey);
        }
        else
        {
            DisplayedPath.Clear();

            foreach (string item in pageKey.Split("/"))
                DisplayedPath.Add(item);
        }
    }
}

internal abstract class NavigationPageVM<TParameter>(INavigationService navigationService) : NavigationPageVM(navigationService)
{
    public TParameter? Parameter { get; set; }

    protected abstract string ParameterRouteKey { get; }

    public virtual new void HandleNavigationBreadcrumBarItemClicked(string[] routes)
    {
        if (routes.Length == 1 && routes[0] == RootPageKey)
            NavigateTo($"{RootPageKey}/Default");
        else if (routes.Length == 2)
            NavigateTo($"{RootPageKey}/{ParameterRouteKey}", Parameter);
        else
            NavigateTo(string.Join('/', routes));
    }

    protected virtual new void NavigateTo(string pageKey, object? parameter = null)
    {
        NavigationService.NavigateTo(pageKey, parameter); // Default page

        if (pageKey == $"{RootPageKey}/Default")
        {
            DisplayedPath.Clear();
            DisplayedPath.Add(RootPageKey);
        }
        else if (pageKey == $"{RootPageKey}/{ParameterRouteKey}" && parameter is TParameter tParameter)
        {
            DisplayedPath.Clear();
            DisplayedPath.Add(RootPageKey);
            DisplayedPath.Add(GetRouteOfParameter(tParameter));
        }
        else
        {
            DisplayedPath.Clear();

            foreach (string item in pageKey.Split("/"))
                DisplayedPath.Add(item);
        }
    }

    protected abstract string GetRouteOfParameter(TParameter tParameter);
}

internal partial class DialogVM : ObservableRecipient, IViewAssociated<ContentDialog>, IDialogParameterAware
{
    public ContentDialog View { get; private set; } = null!;

    public void SetView(object view) => View = (ContentDialog)view;

    public DispatcherQueue Dispatcher { get; set; } = null!;

    public virtual void HandleParameter(object param) => throw new NotImplementedException();

    public virtual void OnLoaded() { }

    public virtual void OnUnloaded() { }

    protected void HideAndGlobalNavigate(string pageKey, object? parameter = null)
    {
        this.View.Hide();
        this.Messenger.Send(new GlobalNavigationMessage(pageKey, parameter));
    }
}

internal partial class DialogVM<TDialog> : DialogVM, IViewAssociated<TDialog>
    where TDialog : ContentDialog
{
    public new TDialog View { get; private set; } = null!;

    public new void SetView(object view) => View = (TDialog)view;
}